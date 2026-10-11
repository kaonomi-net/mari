// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Attributes
{
    using System;

    /// <summary>
    /// Define the input and output associate the vgrain manage
    /// </summary>
    /// <remarks>
    ///     Use <see cref="Framework.NoneType"/> on parse without in ou out
    /// </remarks>
    /// <typeparam name="TInput">The type of the input.</typeparam>
    /// <typeparam name="TOutput">The type of the output.</typeparam>
    /// <seealso cref="Attribute" />
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = true, Inherited = true)]
    public sealed class VGrainUsageAttribute<TInput, TOutput> : Attribute
    {
    }
}
