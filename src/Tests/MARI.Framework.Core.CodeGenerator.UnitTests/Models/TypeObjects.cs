// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.CodeGenerator.UnitTests.Models
{
    using MARI.Framework.Core.Abstractions.Attributes.MetaData;

    [RefSimpleNameIdentifier("simple-class", CodeGenTestConstants.BagNamespace)]
    internal sealed record class SimpleClass();

    [RefSimpleNameIdentifier("simple-struct", CodeGenTestConstants.BagNamespace)]
    internal record struct SimpleStruct();

    [RefSimpleNameIdentifier("classic-class", CodeGenTestConstants.BagNamespace)]
    internal sealed class ClassicClass
    {

    }

    [RefSimpleNameIdentifier("classic-struct", CodeGenTestConstants.BagNamespace)]
    internal struct ClassicStruct
    {

    }

    [RefSimpleNameIdentifier("enum-type", CodeGenTestConstants.BagNamespace)]
    internal enum EnumType
    {

    }
}
