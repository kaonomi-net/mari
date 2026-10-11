// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.Signals.UnitTests
{
    using MARI.Framework.Node.Signals;
    using MARI.UnitTests.ToolKit.Tests;

    /// <summary>
    /// Test for <see cref="SignalHandlerStateSurrogate"/>
    /// </summary>
    public sealed class SignalHandlerStateSurrogateUTest
    {
        #region Fields

        // Contains instead of inherite because tested class are in internal
        private readonly SurrogateBaseTest<SignalHandlerState, SignalHandlerStateSurrogate, SignalHandlerStateSurrogateConverter> _tester;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SignalHandlerStateSurrogateUTest"/> class.
        /// </summary>
        public SignalHandlerStateSurrogateUTest()
        {
            this._tester = new SurrogateBaseTest<SignalHandlerState, SignalHandlerStateSurrogate, SignalHandlerStateSurrogateConverter>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Ensure that <see cref="SignalHandlerStateSurrogate"/> is serializable
        /// </summary>
        [Fact]
        public void Ensure_Surrogate_Serialization()
        {
            this._tester.Ensure_Surrogate_Serialization();
        }

        /// <summary>
        /// Ensures the <see cref="SignalHandlerState"/> is serializable using <see cref="SignalHandlerStateSurrogate"/> and <see cref="SignalHandlerStateSurrogateConverter"/>.
        /// </summary>
        [Fact]
        public void Ensure_Source_Is_Serializable_Using_Surrogate_And_Converter()
        {
            this._tester.Ensure_Source_Is_Serializable_Using_Surrogate_And_Converter();
        }

        #endregion
    }
}
