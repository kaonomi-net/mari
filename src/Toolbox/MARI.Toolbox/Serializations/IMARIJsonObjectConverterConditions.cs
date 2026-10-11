// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Serializations
{
    using System;

    /// <summary>
    /// Define a service able validate if the convertions could apply following the algorithme setups
    /// </summary>
    internal interface IMARIJsonObjectConverterConditions
    {
        /// <summary>
        /// Validates the conditions
        /// </summary>
        bool Validate(Func<string, Tuple<bool, object?>> getJsonValue, Type objectType, object? existingValue);
    }
}
