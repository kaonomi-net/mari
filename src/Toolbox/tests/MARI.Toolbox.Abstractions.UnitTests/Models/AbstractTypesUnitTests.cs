// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Abstractions.UnitTests.Models
{
    using AutoFixture;

    using MARI.Toolbox.Models;
    using MARI.Toolbox.UnitTests.ToolKit.Helpers;
    using MARI.Toolbox.UnitTests.ToolKit.Models;

    using NFluent;

    using Xunit;

    /// <summary>
    /// Test for <see cref="AbstractType"/> and his descendent
    /// </summary>
    public class AbstractTypesUnitTests
    {
        #region Methods

        [Fact]
        public void ConcretType_Serializable()
        {
            Generic_Serializable<ConcretType>();
        }

        [Fact]
        public void CollectionConcretType_Serializable()
        {
            Generic_Serializable<CollectionType>();
        }

        [Fact]
        public void GenericType_Serializable()
        {
            Generic_Serializable<GenericType>();
        }

        [Fact]
        public void GenericRefType_Serializable()
        {
            Generic_Serializable<GenericRefType>();
        }

        #region Tools

        private void Generic_Serializable<TType>()
        {
            var serializationFixture = new Fixture();

            //serializationFixture.Customizations.Add(KnownTypeRelay.Create<AbstractType>());
            serializationFixture.Register<AbstractType>(() => (AbstractType)null!);

            var serializable = ObjectTestHelper.IsSerializable<TType>(supportCyclingReference: true, manualFixture: serializationFixture);
            Check.That(serializable).IsTrue();
        }
        #endregion

        #endregion
    }
}
