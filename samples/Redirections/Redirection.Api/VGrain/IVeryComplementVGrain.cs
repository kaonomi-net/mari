namespace Redirection.Api.VGrain
{
    using MARI.Framework.Core.Abstractions.Attributes;

    [VGrainStatelessWorker]
    public interface IVeryComplementVGrain : IComplementVGrain
    {
    }
}
