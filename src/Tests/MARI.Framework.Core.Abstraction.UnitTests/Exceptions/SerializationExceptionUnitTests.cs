// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstraction.UnitTests.Exceptions
{
    using AutoFixture;

    using MARI.Framework.Core.Abstractions.Exceptions;
    using MARI.UnitTests.ToolKit.Tests;
    using MARI.UnitTests.ToolKit.Xunits;

    using System;

    using Xunit;

    /// <summary>
    /// All exception in assembly MARI.Framework.Core.Abstraction
    /// </summary>
    /// <seealso cref="ExceptionSerializationTester" />
    public sealed class SerializationExceptionUnitTests : ExceptionSerializationTester
    {
        [Theory]
        [ChildTypeData<IMARIException, MARI.Framework.Core.Abstractions.IDefinition>]
        public void Node_Exception_Serialization_Through_Surrogate(Type type)
        {
            base.Exception_Serialization_Through_Surrogate(type);
        }

        protected override void OnSourceCreationSetup<TSource, TSurrogate, TConverter>(Fixture fixture)
        {
            fixture.Register<MARIBaseException>(() => new MARIException("inner error"));

            base.OnSourceCreationSetup<TSource, TSurrogate, TConverter>(fixture);
        }
    }
}
