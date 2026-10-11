// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.UnitTests.Tools
{
    using AutoFixture;

    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Abstractions.Enums;
    using MARI.Framework.Core.Abstractions.Sequence;

    using MARI.Toolbox.Models;
    using MARI.Toolbox.UnitTests.ToolKit.Helpers;

    using System;

    public class TestSequenceStageDefinition : SequenceStageDefinition
    {
        private static readonly Fixture s_globalAutoFixture;

        static TestSequenceStageDefinition()
        {
            s_globalAutoFixture = ObjectTestHelper.PrepareFixture();
        }

        public TestSequenceStageDefinition(Guid uid,
                                           string displayName,
                                           StageTypeEnum type,
                                           AbstractType? input,
                                           AbstractType? output,
                                           DefinitionMetaData? metaData,
                                           bool preventReturn = false)
            : base(uid, displayName, type, input, output, metaData, preventReturn)
        {
        }

        protected override bool OnStageEquals(SequenceStageDefinition other)
        {
            return true;
        }

        protected override int OnStageGetHashCode()
        {
            return 0;
        }

        public static TestSequenceStageDefinition Create(IFixture? fixture = null, Guid? uid = null)
        {
            fixture ??= s_globalAutoFixture;

            return new TestSequenceStageDefinition(uid ?? fixture.Create<Guid>(),
                                                   fixture.Create<string>(),
                                                   fixture.Create<StageTypeEnum>(),
                                                   fixture.Create<AbstractType>(),
                                                   fixture.Create<AbstractType>(),
                                                   fixture.Create<DefinitionMetaData>(),
                                                   fixture.Create<bool>());
        }
    }
}
