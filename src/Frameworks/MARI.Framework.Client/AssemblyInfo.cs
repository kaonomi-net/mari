// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

using MARI.Framework.Client.Configurations.AutoConfigurator;
using MARI.Framework.Cluster.Abstractions.Attributes;
using MARI.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
using MARI.Framework.Node.Configurations.AutoConfigurator;

[assembly: AutoConfiguratorKey("Default")]

[assembly: AutoConfigurator<IMembershipsAutoConfigurator, AutoDefaultClientConfigurator>()]
[assembly: AutoConfigurator<IClusterEndpointAutoConfigurator, AutoDefaultClientEndpointAutoConfigurator>()]