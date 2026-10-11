// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Configurations
{
    using MARI.Framework.Configurations;
    using MARI.Framework.Core.Abstractions;
    using MARI.Framework.Core.Helpers;
    using MARI.Framework.Node.Manifests;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    using Orleans.Serialization.Configuration;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Build mari node virtual grainsystem
    /// </summary>
    /// <seealso cref="IMARINodeVGrainWizard" />
    internal sealed class ClusterNodeVGrainBuilder : IMARINodeVGrainWizard
    {
        #region Fields

        private readonly HashSet<Type> _roRemoveVGrain;
        private readonly Dictionary<Type, Type> _addVGrain;
        private bool _removeAllAutoDiscovered;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterNodeVGrainBuilder"/> class.
        /// </summary>
        public ClusterNodeVGrainBuilder(IMARINodeWizard root)
        {
            ArgumentNullException.ThrowIfNull(root);

            this.Root = root;
            this._roRemoveVGrain = new HashSet<Type>();
            this._addVGrain = new Dictionary<Type, Type>();
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public IMARINodeWizard Root { get; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IMARINodeVGrainWizard Add<TVGrain, TVGrainImplementation>()
            where TVGrain : IVGrain
            where TVGrainImplementation : TVGrain
        {
            return Add(typeof(TVGrain), typeof(TVGrainImplementation));
        }

        /// <inheritdoc />
        public IMARINodeVGrainWizard Add(Type? vgrainInterface, Type? vgrainImplementation)
        {
            ArgumentNullException.ThrowIfNull(vgrainInterface);
            ArgumentNullException.ThrowIfNull(vgrainImplementation);

            this._addVGrain.Add(vgrainInterface, vgrainImplementation);
            return this;
        }

        /// <inheritdoc />
        public IMARINodeVGrainWizard Remove<TVGrain>()
            where TVGrain : IVGrain
        {
            return Remove(typeof(TVGrain));
        }

        /// <inheritdoc />
        public IMARINodeVGrainWizard Remove(Type? vgrain)
        {
            ArgumentNullException.ThrowIfNull(vgrain);

            this._roRemoveVGrain.Add(vgrain);
            return this;
        }

        /// <inheritdoc />
        public IMARINodeVGrainWizard RemoveAllAutoDiscoveryVGrain()
        {
            this._removeAllAutoDiscovered = true;
            return this;
        }

        /// <summary>
        /// Setups vgrains declarations
        /// </summary>
        internal TypeManifestOptions Setup(IServiceCollection services)
        {
            var manifestProviders = services.Where(s => s.ServiceType == typeof(IConfigureOptions<TypeManifestOptions>))
                                            .ToArray();

            var options = new TypeManifestOptions();

            foreach (var manifestProvider in manifestProviders)
            {
                if (manifestProvider.ImplementationType == null)
                    continue;

                var cfg = (IConfigureOptions<TypeManifestOptions>?)Activator.CreateInstance(manifestProvider.ImplementationType, Array.Empty<object>());

                ArgumentNullException.ThrowIfNull(cfg);
                cfg.Configure(options);

                services.Remove(manifestProvider);
            }

            bool implementationVGrainChanged = false;
            var resultCopyImplementations = new HashSet<Type>(options.InterfaceImplementations);

            // Remove all auto discovert no system/framework  vgrain 
            if (this._removeAllAutoDiscovered)
            {
                implementationVGrainChanged = true;
                foreach (var impl in options.InterfaceImplementations)
                {
                    if (VGrainMetaDataHelper.IsSystemVGrain(impl))
                        continue;

                    resultCopyImplementations.Remove(impl);
                }
            }

            if (this._roRemoveVGrain.Any())
            {
                implementationVGrainChanged = true;
                foreach (var impl in this._roRemoveVGrain)
                {
                    if (VGrainMetaDataHelper.IsSystemVGrain(impl))
                        continue;

                    resultCopyImplementations.Remove(impl);
                }
            }

            if (this._addVGrain.Any())
            {
                implementationVGrainChanged = true;
                foreach (var impl in this._addVGrain)
                {
                    options.Interfaces.Add(impl.Key);
                    resultCopyImplementations.Add(impl.Value);
                }
            }

            if (implementationVGrainChanged)
            {
                options.InterfaceImplementations.Clear();
                foreach (var impl in resultCopyImplementations)
                    options.InterfaceImplementations.Add(impl);
            }

            services.AddSingleton<IConfigureOptions<TypeManifestOptions>>(new MARITypeManifestProvider(options));

            return options;
        }

        #endregion
    }
}
