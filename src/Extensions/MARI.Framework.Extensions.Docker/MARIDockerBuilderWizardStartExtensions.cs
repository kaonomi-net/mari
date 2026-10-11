// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

// Keep : MARI.Framework.Configurations; to easy configuration use
namespace MARI.Framework.Configurations
{
    using MARI.Framework.Extensions.Docker;
    using MARI.Framework.Extensions.Docker.Abstractions;
    using MARI.Framework.Extensions.Docker.Abstractions.Options;
    using MARI.Framework.Node.Abstractions.Artifacts;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Extensions use to setup docker artifact environment 
    /// </summary>

    public static class MARIDockerBuilderWizardStartExtensions
    {
        #region Methods

        /// <summary>
        /// Setup Mongo db cluster synchronication
        /// </summary>
        public static IMARINodeWizard EnableDockerHost(this IMARINodeWizard wizard)
        {
            wizard.GetServiceCollection().AddSingleton<DockerMARIEnvironmentService>();
            wizard.AddService<IDockerClientFactory, DockerClientFactory>();
            wizard.AddService<IArtifactExecutorDedicatedFactory, ArtifactExecutorDockerDedicatedFactory>();
            wizard.AddService<IDockerProcessorFactory, DockerProcessorFactory>();
            return wizard;
        }

        /// <summary>
        /// Adds the docker option.
        /// </summary>
        public static IMARINodeWizard AddDockerOption(this IMARINodeWizard wizard, DockerEnvironmentOptions option)
        {
            wizard.AddNodeOption(option);
            return wizard;
        }

        /// <summary>
        /// Adds the docker option.
        /// </summary>
        public static IMARINodeWizard AddDockerOption(this IMARINodeWizard wizard, string optionSectionPath)
        {
            wizard.AddNodeOption<DockerEnvironmentOptions>(optionSectionPath);
            return wizard;
        }

        #endregion
    }
}
