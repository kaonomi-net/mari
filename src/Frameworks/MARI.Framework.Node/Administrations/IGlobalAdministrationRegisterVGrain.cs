// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Administrations
{
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Attributes;

    [VGrainIdSingleton]
    [MARISystemVGrain]
    internal interface IGlobalAdministrationRegisterVGrain : IVGrain
    {
    }
}
