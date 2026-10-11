// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

// KEEP : System.Collections.Generic
namespace System.Collections.Generic
{
    /// <summary>
    /// 
    /// </summary>
    // TODO : Move to MARI.Toolbox
    public static class MARIDictionaryExtensions
    {
        public static TCollection TryGetOrAddContainer<TKey, TCollection>(this IDictionary<TKey, TCollection> host, TKey key)
            where TCollection : System.Collections.IEnumerable, new()
        {
            TCollection? collection = default;
            if (!host.TryGetValue(key, out collection))
            {
                collection = new TCollection();
                host.Add(key, collection);
            }
            return collection;
        }
    }
}
