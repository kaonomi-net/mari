// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Blackboard.Models.Surrogates
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Deferred;
    using MARI.Framework.Core.Abstractions.Repositories;
    using MARI.Framework.Core.Abstractions.Surrogates;
    using MARI.Framework.Node.Blackboard.Abstractions.Models;
    using MARI.Framework.Node.Blackboard.Abstractions.Models.Queries;

    [GenerateSerializer]
    internal record struct BlackboardDeferredQueryStateSurrogate(DeferredId DeferredId,
                                                                 IConcretTypeSurrogate? ResponseType,
                                                                 BlackboardQueryTypeEnum Type,
                                                                 byte[] SerializeQuery);

    [GenerateSerializer]
    internal record struct BlackboardGrainStateSurrogate(BlackboardTemplateDefinitionSurrogate? TemplateDefinition,
                                                         BlackboardId BlackboardId,
                                                         string Name,
                                                         BlackboardRecordRegistryStateSurrogate BlackboardRecordRegistryState,
                                                         BlackboardDeferredQueryStateSurrogate[] Queries,
                                                         BlackboardLifeStatusEnum CurrentLifeStatus,
                                                         SubscriptionId[] SubscriptionIds);

    [RegisterConverter]
    internal sealed class BlackboardGrainStateConverter : IConverter<BlackboardGrainState, BlackboardGrainStateSurrogate>
    {
        #region Fields

        private readonly IMARISerializer _mariSerializer;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BlackboardGrainStateConverter"/> class.
        /// </summary>
        public BlackboardGrainStateConverter(IMARISerializer mariSerializer)
        {
            this._mariSerializer = mariSerializer;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public BlackboardGrainState ConvertFromSurrogate(in BlackboardGrainStateSurrogate surrogate)
        {
            return new BlackboardGrainState((surrogate.TemplateDefinition is not null ? BlackboardTemplateDefinitionConverter.Default.ConvertFromSurrogate(surrogate.TemplateDefinition.Value) : null),
                                            surrogate.BlackboardId,
                                            surrogate.Name,
                                            BlackboardRecordRegistryStateConverter.Default.ConvertFromSurrogate(surrogate.BlackboardRecordRegistryState),
                                            surrogate.Queries?.Select(q => BlackboardDeferredQueryState.Create(q, this._mariSerializer)) ?? EnumerableHelper<BlackboardDeferredQueryState>.ReadOnly,
                                            surrogate.CurrentLifeStatus,
                                            surrogate.SubscriptionIds);
        }

        /// <inheritdoc />
        public BlackboardGrainStateSurrogate ConvertToSurrogate(in BlackboardGrainState value)
        {
            var surrogate = new BlackboardGrainStateSurrogate()
            {
                TemplateDefinition = (value.TemplateCopy is not null ? BlackboardTemplateDefinitionConverter.Default.ConvertToSurrogate(value.TemplateCopy) : null),
                BlackboardId = value.BlackboardId,
                Name = value.Name,
                BlackboardRecordRegistryState = BlackboardRecordRegistryStateConverter.Default.ConvertToSurrogate(value.Registry),
                Queries = value.GetQueries()?.Select(r => r.ToSurrogate()).ToArray() ?? EnumerableHelper<BlackboardDeferredQueryStateSurrogate>.ReadOnlyArray,
                CurrentLifeStatus = value.CurrentLifeStatus,
                SubscriptionIds = value.GetSubscriptions().ToArray()
            };

            return surrogate;
        }

        #endregion
    }
}
