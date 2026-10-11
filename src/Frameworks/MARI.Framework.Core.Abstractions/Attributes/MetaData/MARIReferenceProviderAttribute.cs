// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Attributes.MetaData
{
    using MARI.Framework.Core.Abstractions.References;

    using System;

    /// <summary>
    /// Assembly attribute charge to populate the registry with assembly information
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public abstract class MARIReferenceProviderAttribute : Attribute
    {
        /// <summary>
        /// Populates the specified registry with assemblies references
        /// </summary>
        public abstract void Populate(MARIReferenceRegistry registry);
    }
}
