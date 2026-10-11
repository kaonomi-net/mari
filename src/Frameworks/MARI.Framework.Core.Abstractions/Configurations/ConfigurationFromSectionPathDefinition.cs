// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Configurations
{
    using MARI.Toolbox.Models;

    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [Immutable]
    [Serializable]
    [DataContract]
    [GenerateSerializer]
    [ImmutableObject(true)]
    public sealed class ConfigurationFromSectionPathDefinition<TData> : ConfigurationBaseDefinition
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationDirectDefinition{TData}"/> class.
        /// </summary>
        public ConfigurationFromSectionPathDefinition(Guid uid,
                                                      string displayName,
                                                      string configName,
                                                      string sectionPath,
                                                      TData? defaultData,
                                                      bool secureDataTansfert,
                                                      DefinitionMetaData? definitionMeta = null) 
            : base(uid, displayName, configName, (ConcretBaseType)typeof(TData).GetAbstractType(), secureDataTansfert, definitionMeta)
        {
            this.SectionPath = sectionPath;
            this.DefaultData = defaultData;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the data.
        /// </summary>
        [DataMember]
        [Id(0)]
        public TData? DefaultData { get; }

        /// <summary>
        /// Gets the section path.
        /// </summary>
        [DataMember]
        [Id(1)]
        public string SectionPath { get; }

        #endregion
    }
}
