namespace MARI.Sample.Blackboard.Memory.IVGrains
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Attributes;

    [VGrainStatelessWorker]
    public interface ISumValuesVGrain : IVGrain
    {
        Task<int> Sum(IReadOnlyCollection<int> count, IExecutionContext context);
    }
}
