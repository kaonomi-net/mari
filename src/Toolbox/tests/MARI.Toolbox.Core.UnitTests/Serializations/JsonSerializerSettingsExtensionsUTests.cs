// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Core.UnitTests.Serializations
{
    using AutoFixture;

    using MARI.Toolbox.Serializations;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using NFluent;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Test for <see cref="JsonSerializerSettingsExtensions"/>
    /// </summary>
    public sealed class JsonSerializerSettingsExtensionsUTests
    {
        #region Nested

        public record class RootTest(Guid Uid, NodeTest Test, IReadOnlyCollection<NodeTest> TestCollection, OtherNodeTest OtherTest, IReadOnlyCollection<OtherNodeTest> OtherTestCollection);

        public abstract record class NodeTest(string Type);

        public sealed record class NodeLeftTest(string Value) : NodeTest("LEFT");

        public sealed record class NodeRightTest(int Value) : NodeTest("RIGHT");

        public abstract record class OtherNodeTest(string Type);

        public sealed record class OtherNodeLeftTest(string Value) : OtherNodeTest("OTHER_LEFT");

        public sealed record class OtherNodeRightTest(int Value) : OtherNodeTest("OTHER_RIGHT");

        #endregion

        #region Methods

        /// <summary>
        /// Test discrinimation implementation deserialization
        /// </summary>
        [Fact]
        public void Type_Resolver_Using_Prop_Value()
        {
            var fixture = new Fixture();
            var data = new RootTest(Guid.NewGuid(),
                                    new NodeLeftTest(fixture.Create<string>()),
                                    new NodeTest[]
                                    {
                                        new NodeLeftTest(fixture.Create<string>()),
                                        new NodeRightTest(fixture.Create<int>()),
                                        new NodeLeftTest(fixture.Create<string>()),
                                        new NodeRightTest(fixture.Create<int>())
                                    },
                                    new OtherNodeLeftTest(fixture.Create<string>()),
                                    new OtherNodeTest[]
                                    {
                                        new OtherNodeLeftTest(fixture.Create<string>()),
                                        new OtherNodeRightTest(fixture.Create<int>()),
                                        new OtherNodeLeftTest(fixture.Create<string>()),
                                        new OtherNodeRightTest(fixture.Create<int>())
                                    });

            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.None,
                Formatting = Formatting.Indented
            };

            settings.Configure(b =>
            {
                b.Converter.For<NodeTest>().Where(p => p.Type, "LefT", StringComparer.OrdinalIgnoreCase).ApplyType<NodeLeftTest>()
                                           .Where(p => p.Type, "RIGHT").ApplyType<NodeRightTest>()
                                           .Done

                           .For<OtherNodeTest>().Where(p => p.Type, "OTHER_LEFT", StringComparer.OrdinalIgnoreCase).ApplyType<OtherNodeLeftTest>()
                                                .Where(p => p.Type, "OTHER_RIGHT").ApplyType<OtherNodeRightTest>();

            });

            var dataJson = JsonConvert.SerializeObject(data, settings);
            var reloadData = JsonConvert.DeserializeObject<RootTest>(dataJson, settings);

            Check.That(reloadData).IsNotNull();
            Check.That(reloadData!.Uid).IsEqualTo(data.Uid);

            Check.That(reloadData!.Test).IsEqualTo(data.Test);
            Check.That(reloadData!.TestCollection).ContainsExactly(data.TestCollection);
            
            Check.That(reloadData!.OtherTest).IsEqualTo(data.OtherTest);
            Check.That(reloadData!.OtherTestCollection).ContainsExactly(data.OtherTestCollection);
        }

        #endregion
    }
}
