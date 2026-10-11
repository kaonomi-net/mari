// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

using MARI.Framework.Cluster.Abstractions.Attributes;
using MARI.Framework.Cluster.Abstractions.Configurations.AutoConfigurator;
using MARI.Framework.Extensions.Mongo.Configurations.AutoConfigurator;
using MARI.Framework.Node.Abstractions.Configurations.AutoConfigurator;

[assembly: AutoConfiguratorKey("mongo")]

// Cluster
[assembly: AutoConfigurator<IMembershipsAutoConfigurator, AutoMembershipsMongoConfigurator>()]

// Storage
[assembly: AutoConfigurator<INodeMARIMemoryAutoConfigurator, AutoMARIMongoConfigurator>()]
[assembly: AutoConfigurator<INodeMARIAdminMemoryAutoConfigurator, AutoMARIAdminMongoConfigurator>()]
[assembly: AutoConfigurator<INodeDefaultMemoryAutoConfigurator, AutoDefaultMemoryMongoConfigurator>()]
[assembly: AutoConfigurator<INodeReminderStateMemoryAutoConfigurator, AutoReminderMongoConfigurator>()]
[assembly: AutoConfigurator<INodeCustomGrainMemoryAutoConfigurator, AutoCustomGrainMemoryMongoConfigurator>()]
[assembly: AutoConfigurator<INodeCustomDefinitionProviderAutoConfigurator, AutoCustomDefinitionProviderMongoConfigurator>()]
[assembly: AutoConfigurator<INodeMARIDynamicDefinitionsMemoryAutoConfigurator, AutoMARIDynamicDefinitionsMongoConfigurator>()]

// Repository
[assembly: AutoConfigurator<INodeCustomRepositoryMemoryAutoConfigurator, AutoCustomRepositoryMongoConfigurator>()]
[assembly: AutoConfigurator<INodeDefaultRepositoryMemoryAutoConfigurator, AutoDefaultRepositoryMongoConfigurator>()]
