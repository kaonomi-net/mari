// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Surrogates
{
    using MARI.Toolbox;

    [GenerateSerializer]
    public record struct NoneTypeSurrogate();

    [RegisterConverter]
    public sealed class NoneTypeSurrogateConverter : IConverter<NoneType, NoneTypeSurrogate>
    {
        /// <inheritdoc />
        public NoneType ConvertFromSurrogate(in NoneTypeSurrogate surrogate)
        {
            return NoneType.Instance;
        }

        /// <inheritdoc />
        public NoneTypeSurrogate ConvertToSurrogate(in NoneType value)
        {
            return new NoneTypeSurrogate();
        }
    }
}
