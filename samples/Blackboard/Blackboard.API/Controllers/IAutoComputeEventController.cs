namespace MARI.Sample.Blackboard.Memory.Controllers
{
    using MARI.Framework.Node.Blackboard.Abstractions.VGrains.Controllers;
    using MARI.Sample.Blackboard.Memory.Models;

    public interface IAutoComputeEventController : IBlackboardBaseControllerGrain<AutoComputeBlackboardOptions>, IBlackboardEventControllerGrain
    {
    }
}
