// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.UnitTests.Services
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Attributes;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using System.Reflection;

    internal static class VGrainIdFormaterExtensions
    {
        public static IVGrainId BuildNewId<TGrain>(this IVGrainIdDedicatedFactory grainIdDedicatedFactory, object? input, IExecutionContext ctx, ILogger? logger = null)
        {
            var grainType = typeof(TGrain);
            return grainIdDedicatedFactory.BuildNewId(grainType.GetCustomAttribute<VGrainIdBaseFormatorAttribute>() ?? throw new ArgumentNullException("Missing formator attribute"), grainType, input, ctx, logger ?? NullLogger.Instance);
        }
    }
}
