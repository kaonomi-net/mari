// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Signals.Doors
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Doors;
    using MARI.Framework.Node.Services;

    using MARI.Toolbox.Extensions;
    using MARI.Toolbox.Helpers;

    using Microsoft.Extensions.Logging;

    using Orleans;
    using Orleans.Runtime;

    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Virtual Grain Service in charge to manager the door initialization and lifecycle
    /// </summary>
    /// <seealso cref="GrainService" />
    /// <seealso cref="IDoorVGrainService" />
    internal sealed class DoorVGrainService : MARIVGrainService, IDoorVGrainService
    {
        #region Fields

        private readonly SemaphoreSlim _locker;

        private readonly IDoorDefinitionProvider _doorDefinitionProvider;
        private readonly IGrainFactory _grainFactory;

        #endregion

        #region Ctor 

        /// <summary>
        /// Initializes a new instance of the <see cref="DoorVGrainService"/> class.
        /// </summary>
        public DoorVGrainService(GrainId grainId,
                                 Silo silo,
                                 ILoggerFactory loggerFactory,
                                 IGrainOrleanFactory grainFactory,
                                 IDoorDefinitionProvider doorDefinitionProvider)
            : base(grainId, silo, loggerFactory)
        {
            this._locker = new SemaphoreSlim(1);

            this._grainFactory = grainFactory;
            this._doorDefinitionProvider = doorDefinitionProvider;

            this._doorDefinitionProvider.DataChanged -= DoorDefinitionProvider_DataChanged;
            this._doorDefinitionProvider.DataChanged += DoorDefinitionProvider_DataChanged;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override async Task RefreshInfoAsync()
        {
            await this._locker.WaitAsync();
            try
            {
                await LaunchSignalsVGrainsAsync();
            }
            finally
            {
                this._locker.Release();
            }
        }

        /// <inheritdoc />
        public override Task Stop()
        {
            this._doorDefinitionProvider.DataChanged -= DoorDefinitionProvider_DataChanged;
            return base.Stop();
        }

        /// <summary>
        /// Doors the definition provider data changed.
        /// </summary>
        private async void DoorDefinitionProvider_DataChanged(object? sender, IReadOnlyCollection<Guid> definitionThatChanged)
        {
            // async void : no exception must escape, otherwise the process crash
            try
            {
                await this._locker.WaitAsync();
                try
                {
                    await LaunchSignalsVGrainsAsync(definitionThatChanged);
                }
                finally
                {
                    this._locker.Release();
                }
            }
            catch (Exception ex)
            {
                this.Logger.OptiLog(LogLevel.Error, "Door definitions update failed : {exception}", ex);
            }
        }

        #region Tools

        /// <summary>
        /// Launch door handler base on definition
        /// </summary>
        private async Task LaunchSignalsVGrainsAsync(IReadOnlyCollection<Guid>? definitionThatChanged = null)
        {
            try
            {
                using (var diposableTokenConteneur = CancellationHelper.DisposableTimeout(TimeSpan.FromMinutes(1)))
                using (var grainCancellationToken = diposableTokenConteneur.Content.ToGrainCancellationTokenSource())
                {
                    IEnumerable<DoorDefinition> allDoordDefinitions = await this._doorDefinitionProvider.GetAllValuesAsync(diposableTokenConteneur.Content);

                    if (definitionThatChanged is not null && allDoordDefinitions.Any())
                        allDoordDefinitions = allDoordDefinitions.Where(d => definitionThatChanged.Contains(d.Uid));

                    var initDoorTasks = allDoordDefinitions.Select(async doorDefinition =>
                    {
                        var grainId = doorDefinition.DoorId.Uid;

                        try
                        {
                            var vgrainInterfaceType = Type.GetType(doorDefinition.VGrainInterfaceFullName, true, true);

                            var grain = this._grainFactory.GetGrain(vgrainInterfaceType, grainId);
                            await grain.AsReference<IDoorVGrain>().UpdateAsync(doorDefinition, grainCancellationToken.Token);
                        }
                        catch (OperationCanceledException)
                        {

                        }
                        catch (Exception ex)
                        {
                            this.Logger.OptiLog(LogLevel.Critical,
                                                 "Could not start door {uid} - {name} due to {exception}",
                                                 doorDefinition.Uid,
                                                 doorDefinition.Name,
                                                 ex);
                        }
                    }).ToArray();

                    await initDoorTasks.SafeWhenAllAsync(diposableTokenConteneur.Content);
                }
            }
            catch (Exception ex)
            {
                this.Logger.OptiLog(LogLevel.Error, "VGrain Service Start Failed : {exception}", ex);
            }
        }

        #endregion

        #endregion
    }
}
