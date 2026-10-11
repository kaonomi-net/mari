namespace DynamicDefinition.Api.VGrains
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Attributes;

    [VGrainIdSingleton]
    [VGrainStatelessWorker]
    public interface ITextActionVGrain : IVGrain
    {
        Task<string> Concat(string source, IExecutionContext<string> executionContext);
    }
}
