// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace Kaonomi.Sample.Forex.VGrain.Abstractions
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Attributes;
    using MARI.Framework.Core.Abstractions.Enums;
    using MARI.Framework.Core.Abstractions.Models;

    using System.Threading.Tasks;

    /// <summary>
    /// Grain in charge to store and treat currency pair value
    /// </summary>
    /// <seealso cref="IVGrain" />
    [VGrainIdFormat(IdFormatTypeEnum.String, FirstParameterTemplate = "{executionContext.configuration}", FirstParameterFallback = "global")]
    public interface ICurrencyPairVGrain : IVGrain
    {
        /// <summary>
        /// Stores historical values
        /// </summary>
        Task StoreAsync(SerieSetValueModel<double?, string> input, IExecutionContext<string> executionContext);

        /// <summary>
        /// Gets the last values.
        /// </summary>
        Task<IReadOnlyCollection<SerieSetValueModel<double?, string>>> GetLastValues(int quantity, IExecutionContext<string> executionContext);
    }
}
