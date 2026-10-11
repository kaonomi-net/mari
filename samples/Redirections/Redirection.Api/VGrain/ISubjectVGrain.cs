namespace Redirection.Api.VGrain
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Attributes;

    using Redirection.Api.Models;

    [VGrainStatelessWorker]
    public interface ISubjectVGrain : IVGrain
    {
        Task<TBuilder> PopulateSubject<TBuilder>(TBuilder input, IExecutionContext ctx) where TBuilder : ITextBuilder;
    }
}
