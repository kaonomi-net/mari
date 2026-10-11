// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    using Orleans.Messaging;

    /// <summary>
    /// Wizard used to setup cluster configuration
    /// </summary>
    public interface IMARIClusterBuilder : IBuilderMARIBaseWizard, IMARIBaseGenericBuilder
    {
        #region Methods

        /// <summary>
        /// Customizes the cluster identifier.
        /// </summary>
        /// <param name="clusterId">The cluster identity. This used to be called DeploymentId before Orleans 2.0 name.</param>
        /// <param name="serviceId">Unique identifier for this service, which should survive deployment and redeployment, where as <see cref="ClusterId"/> might not.</param>
        IMARIClusterBuilder CustomizeClusterId(string serviceId, string clusterId);

        /// <summary>
        /// [CLIENT] Adds the gateway list provider, used for the client to get vgrain list from cluster.
        /// </summary>
        IMARIClusterBuilder AddGatewayListProvider<TListProvider>() where TListProvider : class, IGatewayListProvider;

        /// <summary>
        /// Adds the configuration validator.
        /// </summary>
        IMARIClusterBuilder AddConfigurationValidator<TValidator>() where TValidator : class, IConfigurationValidator;

        /// <summary>
        /// [SERVER] Adds the membership table managment service.
        /// </summary>
        /// <remarks>
        ///     This service is responsible of the node synchronisation that form a cluster bubble
        /// </remarks>
        IMARIClusterBuilder AddMembershipTable<TMembership>() where TMembership : class, IMembershipTable;

        #endregion
    }
}
