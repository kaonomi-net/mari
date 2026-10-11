// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Signals.UnitTests
{
    using AutoFixture;

    using MARI.Framework.Core.Abstractions.Doors;
    using MARI.Framework.Node.Signals.Doors;
    using MARI.Framework.Node.Signals.Models;
    using MARI.UnitTests.ToolKit.Tests;

    using MARI.Toolbox.UnitTests.ToolKit.Helpers;

    /// <summary>
    /// Test for <see cref="DoorHandlerStateSurrogate"/>
    /// </summary>
    public sealed class DoorHandlerStateSurrogateUTest : SurrogateBaseTest<DoorHandlerState, DoorHandlerStateSurrogate, DoorHandlerStateSurrogateConverter>
    {
        #region Methods

        /// <inheritdoc />
        protected override DoorHandlerState SourceCreation(Fixture fixture)
        {
            fixture = ObjectTestHelper.PrepareFixture(supportMutableValueType: true, supportCyclingReference: true);

            var state = new DoorHandlerState(fixture.CreateMany<DoorSignalReceivedStatusSurrogate>(), null);

            var def = fixture.Create<BooleanLogicalDoorDefinition>();

            state.InitializeSignalSupport(def);

            return state;
        }

        #endregion
    }
}
