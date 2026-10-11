// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Attributes
{
    using System;

    /// <summary>
    /// Generate surrogate struct to be able to manage serialization and restoration by CTOR
    /// </summary>
    /// <remarks>
    ///     Auto-Gen surrogate class through code generator MARI.Framework.Generator if surrogate class doesn't exists
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AutoGenerateSerializerByCtorAttribute : Attribute
    {
    }
}
