// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Attributes
{
    using System;

    /// <summary>
    /// Define the grain as stateless worker used by orlean and mari to optimize the usage
    /// </summary>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class VGrainStatelessWorkerAttribute : Attribute
    {
    }
}
