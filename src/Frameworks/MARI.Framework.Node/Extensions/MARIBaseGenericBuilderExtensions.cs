// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    using System;

    public static class MARIBaseGenericBuilderExtensions
    {
        /// <summary>
        /// Safe way to convert from <see cref="IMARIBaseGenericBuilder.SourceOrleanBuilder"/> to <see cref="ISiloBuilder"/>
        /// </summary>
        public static ISiloBuilder GetSiloBuilder(this IMARIBaseGenericBuilder wizard)
        {
            ArgumentNullException.ThrowIfNull(wizard);

            // Prever type case before instead on allocating inline variable in condition if it's use outside the scope

#pragma warning disable IDE0019 // Use pattern matching
            var siloBuilder = wizard.SourceOrleanBuilder as ISiloBuilder;
#pragma warning restore IDE0019 // Use pattern matching

            if (wizard.IsClient || siloBuilder == null)
                throw new InvalidOperationException("The auto configurator must only be used by Node/Server side");

            return siloBuilder;
        }
    }
}
