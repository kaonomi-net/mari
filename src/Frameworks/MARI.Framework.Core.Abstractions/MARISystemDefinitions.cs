// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions
{
    using MARI.Framework.Core.Abstractions.Signals;

    /// <summary>
    /// Define system definition used through the mari system
    /// </summary>
    public static class MARISystemDefinitions
    {
        public static class Signals
        {
            #region Fields

            internal static readonly SignalDefinition s_dynamicDefinitionChanged;

            #endregion

            #region Ctor

            /// <summary>
            /// Initializes a new instance of the <see cref="Signals"/> class.
            /// </summary>
            static Signals()
            {
                s_dynamicDefinitionChanged = new SignalDefinition(new Guid("F6B9F91B-6B2A-48D4-BED5-EEEDEE71F2DD"),
                                                                  RefIdHelper.Generate(Enums.RefTypeEnum.Signal, "dynamic-definition-changed", "mari.framework.system"),
                                                                  "DynamicDefinitionChanged",
                                                                  new DefinitionMetaData("Signal send to synchronize silo local dynamic definition cache.",
                                                                                         nameof(MARI) + "/" + "System",
                                                                                         new[] { nameof(MARI), "System" },
                                                                                         DateTime.UtcNow,
                                                                                         "mari.framework.system"));
                DynamicDefinitionChanged = s_dynamicDefinitionChanged.SignalId;
            }

            #endregion

            #region Properties

            /// <summary>
            /// Signal raised when a dynamic definition is added or removed
            /// </summary>
            /// <remarks>
            ///     The signal carry a <see cref="CollectionChangeSignalMessage{Guid}"/> with change informations
            /// </remarks>
            public static SignalId DynamicDefinitionChanged { get; }

            #endregion
        }

        /// <summary>
        /// Gets all system definitions.
        /// </summary>
        internal static IEnumerable<IDefinition> GetAllSystemDefinitions()
        {
            yield return MARISystemDefinitions.Signals.s_dynamicDefinitionChanged;
        }
    }
}
