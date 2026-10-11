namespace MARI.Sample.Blackboard.Memory.IVGrains
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Attributes;
    using MARI.Framework.Core.Abstractions.Enums;
    using MARI.Framework.Node.Blackboard.Abstractions.Models;
    using MARI.Sample.Blackboard.Memory.Models;

    using System.Threading.Tasks;

    [VGrainIdFormat(IdFormatTypeEnum.String, FirstParameterTemplate = "{executionContext.Configuration}")]
    public interface IStoreVGrain : IVGrain
    {
        Task<Guid> PrepareSlotAsync(string logicalType, IExecutionContext<string> ctx);

        Task PushNewValueAsync<TVal>(TVal val, IExecutionContext<string> ctx);

        Task<IReadOnlyCollection<RecordDisplay>> GetAllValuesAsync(IExecutionContext<string> ctx);

        Task<IReadOnlyCollection<MetaDataRecordContainer>> GetAllMetaDataAsync(IExecutionContext<string> ctx);
    }
}
