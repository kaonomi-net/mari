// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Test.Interfaces
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Attributes;

    using System.Threading.Tasks;

    /// <summary>
    /// Test VGrain that extract html tag in a html code
    /// </summary>
    /// <seealso cref="IVGrain" />
    [VGrainUsage<Tag, TagQualify>]
    public interface ITagQualifierTestTransformer : IVGrain
    {
        /// <summary>
        /// Extracts tags from HTML asynchronous.
        /// </summary>
        Task<TagQualify> QualifyTagAsync(Tag tag, IExecutionContext sequenceContext);
    }
}
