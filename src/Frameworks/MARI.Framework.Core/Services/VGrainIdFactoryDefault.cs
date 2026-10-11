// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Services
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Enums;
    using MARI.Framework.Core.Helpers;
    using MARI.Framework.Core.Models;
    using MARI.Toolbox.Disposables;

    using Microsoft.Extensions.Logging;

    using Orleans.Runtime;

    using System;

    /// <summary>
    /// Factory implementation used to generate singleton <see cref="GrainId"/>
    /// </summary>
    /// <seealso cref="IVGrainIdFactory" />
    public sealed class VGrainIdFactoryDefault : SafeDisposable, IVGrainIdDefaultFactory
    {
        #region Fields

        private static readonly VGrainIdFactorySingleton s_singletonInst = new VGrainIdFactorySingleton();

        #endregion

        #region Methods

        /// <inheritdoc />
        public IVGrainId BuildNewId(Type vgrainType,
                                    object? input,
                                    IExecutionContext? executionContext,
                                    ILogger? logger = null)
        {
            object primary;
            string? extensionKey = null;

            var metaData = VGrainMetaDataHelper.GetVGrainMetaDataType(vgrainType);

            var format = IdFormatTypeEnum.String;

            if (metaData.IdFormatTypes.Any())
                format = metaData.IdFormatTypes.First();

            if (metaData.IsStatelessWorker)
            {
                // Stateless worker will be automatically managed by orlean as pool worker, to prevent many worker pop it's more optimized to have a singleton id
                return s_singletonInst.BuildNewId(format, vgrainType);
            }

            switch (format)
            {
                case IdFormatTypeEnum.Guid:
                    primary = Guid.NewGuid();
                    break;

                case IdFormatTypeEnum.CompositionGuidString:
                    primary =  Guid.NewGuid();
                    extensionKey = Guid.NewGuid().ToString();
                    break;

                case IdFormatTypeEnum.CompositionStringString:
                    primary =  Guid.NewGuid().ToString();
                    extensionKey = Guid.NewGuid().ToString();
                    break;

                case IdFormatTypeEnum.Long:
                    primary = Random.Shared.Next(0, int.MaxValue);
                    break;

                case IdFormatTypeEnum.CompositionLongString:
                    primary =  Random.Shared.Next(0, int.MaxValue);
                    extensionKey = Guid.NewGuid().ToString();
                    break;

                case IdFormatTypeEnum.String:
                default:
                    primary =  Guid.NewGuid().ToString();
                    break;
            }

            var vgrainId = new VGrainId(vgrainType, format, primary!, extensionKey);
            return vgrainId;
        }

        #endregion
    }
}
