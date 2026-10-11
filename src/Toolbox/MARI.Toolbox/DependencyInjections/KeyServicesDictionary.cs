// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

// KEEP : Microsoft.Extensions.DependencyInjection
namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    // https://stackoverflow.com/questions/77559201/how-to-get-a-dictionary-of-keyed-services-in-asp-net-core-8/77559901#77559901

    // License: This code is published under the MIT license.
    // Source: https://stackoverflow.com/questions/77559201/
    public static class KeyedServiceExtensions
    {
        /// <summary>
        /// Allows the resolving keyed services as <see cref="IKeyServicesReadOnlyDictionary{TKey, TService}"/>
        /// </summary>
        public static void AllowResolvingKeyedServicesAsDictionary(this IServiceCollection sc)
        {
            // KeyedServiceCache caches all the keys of a given type for a
            // specific service type. By making it a singleton we only have
            // determine the keys once, which makes resolving the dict very fast.
            sc.AddSingleton(typeof(KeyedServiceCache<,>));

            // KeyedServiceCache depends on the IServiceCollection to get
            // the list of keys. That's why we register that here as well, as it
            // is not registered by default in MS.DI.
            sc.AddSingleton(sc);

            // Last we make the registration for the dictionary itself, which maps
            // to our custom type below. This registration must be  transient, as
            // the containing services could have any lifetime and this registration
            // should by itself not cause Captive Dependencies.
            sc.AddTransient(typeof(IKeyServicesReadOnlyDictionary<,>), typeof(KeyedServiceDictionary<,>));
        }

        // We inherit from ReadOnlyDictionary, to disallow consumers from changing
        // the wrapped dependencies while reusing all its functionality. This way
        // we don't have to implement IDictionary<T,V> ourselves; too much work.
        private sealed class KeyedServiceDictionary<TKey, TService> : ReadOnlyDictionary<TKey, TService>, IKeyServicesReadOnlyDictionary<TKey, TService>
            where TKey : notnull
            where TService : notnull
        {
            #region Ctor            

            /// <summary>
            /// Initializes a new instance of the <see cref="KeyedServiceDictionary{TKey, TService}"/> class.
            /// </summary>
            public KeyedServiceDictionary(KeyedServiceCache<TKey, TService> keys, IServiceProvider provider)
                : base(Create(keys, provider))
            {
                    
            }

            #endregion

            #region Methods

            /// <summary>
            /// Creates the specified directionary base on <see cref="KeyedServiceCache{TKey, TService}"/> informations
            /// </summary>
            private static Dictionary<TKey, TService> Create(KeyedServiceCache<TKey, TService> keys, IServiceProvider provider)
            {
                var dict = new Dictionary<TKey, TService>(capacity: keys.Keys.Length);

                foreach (var key in keys.Keys)
                    dict[key] = provider.GetRequiredKeyedService<TService>(key);

                return dict;
            }

            #endregion
        }

        private sealed class KeyedServiceCache<TKey, TService>
            where TKey : notnull
            where TService : notnull
        {
            #region Fields

            private static readonly Type s_serviceTrait = typeof(TService);
            private static readonly Type s_keyTrait = typeof(TKey);

            #endregion

            #region Ctor

            /// <summary>
            /// Initializes a new instance of the <see cref="KeyedServiceCache{TKey, TService}"/> class.
            /// </summary>
            public KeyedServiceCache(IServiceCollection sc)
            {
                this.Keys = (from service in sc
                             where service.ServiceKey != null
                             where service.ServiceKey!.GetType() == s_keyTrait
                             where service.ServiceType == s_serviceTrait || (service.ServiceType.IsGenericType && 
                                                                             s_serviceTrait.IsGenericType && 
                                                                             service.ServiceType.GetGenericTypeDefinition() == s_serviceTrait.GetGenericTypeDefinition())
                             select (TKey)service.ServiceKey!)
                             .ToArray();
            }

            #endregion

            #region Properties

            /// <summary>
            /// Once this class is resolved, all registrations are guaranteed to be
            /// made, so we can, at that point, safely iterate the collection to get
            /// the keys for the service type.
            /// </summary>
            public TKey[] Keys { get; }

            #endregion
        }
    }
}
