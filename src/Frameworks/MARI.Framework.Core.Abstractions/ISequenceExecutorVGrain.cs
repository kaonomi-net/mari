// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions
{
    using MARI.Framework.Core.Abstractions.Attributes;
    using MARI.Framework.Core.Abstractions.Enums;

    /// <summary>
    /// Define a instance available to execute a <see cref="SequenceDefinition"/>
    /// </summary>

    [VGrainIdFormat(IdFormatTypeEnum.Guid,
                    FirstParameterTemplate = "{executionContext." + nameof(IExecutionContext.CurrentExecutionId) + "}")]

    public interface ISequenceExecutorVGrain : IVGrain, IGrainWithGuidKey, IGenericContextedExecutor<Guid>
    {
    }
}
