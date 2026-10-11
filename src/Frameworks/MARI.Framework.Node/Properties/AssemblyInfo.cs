// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

using MARI.Framework.Cluster.Abstractions.Attributes;
using MARI.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
using MARI.Framework.Node.Abstractions.Configurations.AutoConfigurator;
using MARI.Framework.Node.Configurations.AutoConfigurator;

[assembly: AutoConfiguratorKey("Default")]

//Cluster
[assembly: AutoConfigurator<IMembershipsAutoConfigurator, AutoDefaultMembershipsNodeConfigurator>()]
[assembly: AutoConfigurator<IClusterEndpointAutoConfigurator, AutoDefaultNodeEndpointAutoConfigurator>()]

// Storage
[assembly: AutoConfigurator<INodeDefaultMemoryAutoConfigurator, AutoDefaultMemoryConfigurator>()]
[assembly: AutoConfigurator<INodeMARIMemoryAutoConfigurator, AutoDefaultMARIMemoryConfigurator>()]
[assembly: AutoConfigurator<INodeCustomGrainMemoryAutoConfigurator, AutoDefaultCustomGrainMemoryConfigurator>()]
[assembly: AutoConfigurator<INodeMARIAdminMemoryAutoConfigurator, AutoDefaultMARIAdminMemoryConfigurator>()]
[assembly: AutoConfigurator<INodeReminderStateMemoryAutoConfigurator, AutoDefaultReminderStateMemoryAutoConfigurator>()]
[assembly: AutoConfigurator<INodeMARIDynamicDefinitionsMemoryAutoConfigurator, AutoDefaultMARIDynamicDefinitionsMemoryConfigurator>()]

// Repository
[assembly: AutoConfigurator<INodeDefaultRepositoryMemoryAutoConfigurator, AutoDefaultRepositoryMemoryConfigurator>()]
[assembly: AutoConfigurator<INodeCustomRepositoryMemoryAutoConfigurator, AutoDefaultCustomRepositoryMemoryConfigurator>()]