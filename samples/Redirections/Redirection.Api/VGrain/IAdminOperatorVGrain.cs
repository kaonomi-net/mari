namespace Redirection.Api.VGrain
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Attributes;
    using MARI.Framework.Core.Abstractions.Customizations;

    [VGrainIdSingleton]
    public interface IAdminOperatorVGrain : IVGrain
    {
        Task<IReadOnlyCollection<VGrainRedirectionDefinition>> GetAllRedirection(IExecutionContext executionContext);

        Task<bool> ApplyGlobalComplementRedirection(IExecutionContext executionContext);
        Task<bool> ApplyGlobalConjuctionRedirection(IExecutionContext executionContext);

        Task<bool> ClearAllRedirection(IExecutionContext executionContext);

        Task<bool> ClearGlobalConjuctionRedirection(IExecutionContext executionContext);

        Task<bool> ClearGlobalComplementRedirection(IExecutionContext executionContext);
    }
}
