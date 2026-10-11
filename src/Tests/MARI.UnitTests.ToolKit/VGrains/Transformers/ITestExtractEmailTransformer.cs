// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.UnitTests.ToolKit.VGrains.Transformers
{
    using MARI.Framework.Core.Abstractions;

    using System.Threading.Tasks;

    /// <summary>
    /// Simple test transformer that extract email from a string
    /// </summary>
    public interface ITestExtractEmailTransformer : IVGrain
    {
        /// <summary>
        /// Extra email from <paramref name="input"/>
        /// </summary>
        Task<string[]> ExtractEmailsAsync(string input, IExecutionContext context);
    }
}
