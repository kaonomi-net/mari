// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Node.UnitTests
{
    using MARI.Framework.Node.Configurations;

    using NFluent;

    /// <summary>
    /// Unit test about <see cref="MARINode"/>
    /// </summary>
    public sealed class ClusterNodeUnitTests
    {
        /// <summary>
        /// ClusterNode ctor.
        /// </summary>
        [Fact]
        public async Task ClusterNode_Ctor()
        {
            await using (var node = MARINode.Create(cfg => cfg.SetupClusterOption(opt => opt.BlockAutoConfig())
                                                                   .WizardConfig()
                                                                   .NoCluster()))
            {
            }
        }

        /// <summary>
        /// ClusterNode manually config
        /// </summary>
        [Fact]
        public async Task ClusterNode_Manually()
        {
            bool haveBeenConfig = false;

            var node = MARINode.Create(cfg =>
            {
                cfg.SetupClusterOption(opt => opt.BlockAutoConfig())
                   .WizardConfig()
                   .NoCluster();

                var siloConfig = cfg.ManualyAdvancedConfig();
                Check.That(siloConfig).IsNotNull();

                haveBeenConfig = true;
            });

            await using (node)
            {
            }

            Check.That(haveBeenConfig).IsTrue();
        }
    }
}