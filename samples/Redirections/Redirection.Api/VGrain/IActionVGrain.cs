namespace Redirection.Api.VGrain
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Attributes;

    using Redirection.Api.Models;

    [VGrainStatelessWorker]
    public interface IActionVGrain : IVGrain
    {
        Task<TBuilder> PopulateAction<TBuilder>(TBuilder input, IExecutionContext ctx) where TBuilder : ITextBuilder;
    }
}
