// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstraction.UnitTests.Models
{
    using MARI.Framework.Core.Abstractions.Models;

    using MARI.Toolbox.UnitTests.ToolKit.Helpers;

    /// <summary>
    /// Test for <see cref="VGrainMetaData"/>
    /// </summary>
    public sealed class VGrainMetaDataUTest
    {
        /// <summary>
        /// Ensures the structure <see cref="VGrainMetaData"/> is serializable.
        /// </summary>
        [Fact]
        public void Ensure_VGrainMetaData_IsSerializable()
        {
            ObjectTestHelper.IsSerializable<VGrainMetaData>();
        }
    }
}
