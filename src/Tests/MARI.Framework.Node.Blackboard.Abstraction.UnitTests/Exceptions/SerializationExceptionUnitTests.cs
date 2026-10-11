// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstraction.UnitTests.Exceptions
{
    using AutoFixture;

    using MARI.Framework.Core.Abstractions.Exceptions;
    using MARI.Framework.Node.Blackboard.Abstractions.Models;
    using MARI.UnitTests.ToolKit.Tests;
    using MARI.UnitTests.ToolKit.Xunits;

    using System;

    using Xunit;

    /// <summary>
    /// All exception in assembly MARI.Framework.Node.Blackboard.Abstractions
    /// </summary>
    /// <seealso cref="ExceptionSerializationTester" />
    public sealed class SerializationExceptionUnitTests : ExceptionSerializationTester
    {
        #region Methods

        [Theory]
        [ChildTypeData<IMARIException, MARI.Framework.Node.Blackboard.Abstractions.Exceptions.BlackboardCommandExecutionException>]
        public void Node_Exception_Serialization_Through_Surrogate(Type type)
        {
            base.Exception_Serialization_Through_Surrogate(type);
        }

        #endregion

        #region Tools

        /// <inheritdoc />
        protected override void OnSourceCreationSetup<TSource, TSurrogate, TConverter>(Fixture fixture)
        {
            fixture.Register<DataRecordContainer>(() =>
            {
                return new DataRecordContainer<double>(fixture.Create<string>(),
                                                       fixture.Create<Guid>(),
                                                       fixture.Create<string>(),
                                                       fixture.Create<double>(),
                                                       RecordStatusEnum.Ready,
                                                       fixture.Create<DateTime>(),
                                                       fixture.Create<string>(),
                                                       fixture.Create<DateTime>(),
                                                       fixture.Create<string>(),
                                                       null);
            });

            base.OnSourceCreationSetup<TSource, TSurrogate, TConverter>(fixture);
        }

        #endregion
    }
}
