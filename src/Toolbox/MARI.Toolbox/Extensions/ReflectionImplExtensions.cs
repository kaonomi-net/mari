// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

// Keep : System.Reflection
namespace System.Reflection
{
    using MARI.Toolbox.Collections;

    public static class ReflectionImplExtensions
    {
        #region Fields

        private static readonly IDictionary<Type, IMultiKeyDictionary<Type, Type>> s_typeCache;
        private static readonly ReaderWriterLockSlim s_typeCacheLocker;

        private static readonly IDictionary<MethodInfo, IMultiKeyDictionary<Type, MethodInfo>> s_methodCache;
        private static readonly ReaderWriterLockSlim s_methodCacheLocker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ReflectionImplExtensions"/> class.
        /// </summary>
        static ReflectionImplExtensions()
        {
            s_typeCache = new Dictionary<Type, IMultiKeyDictionary<Type, Type>>(42);
            s_typeCacheLocker = new ReaderWriterLockSlim();

            s_methodCache = new Dictionary<MethodInfo, IMultiKeyDictionary<Type, MethodInfo>>(42);
            s_methodCacheLocker = new ReaderWriterLockSlim();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Makes the generic type from arguments with cache result.
        /// </summary>
        public static Type MakeGenericTypeWithCache(this Type originalType, params Type[] arguments)
        {
            return MakeGenericTypeWithCache(originalType, (IReadOnlyCollection<Type>)arguments);
        }

        /// <summary>
        /// Makes the generic type from arguments with cache result.
        /// </summary>
        public static Type MakeGenericTypeWithCache(this Type originalType, IReadOnlyCollection<Type> arguments)
        {
            s_typeCacheLocker.EnterReadLock();
            try
            {
                if (s_typeCache.TryGetValue(originalType, out var specializedTypeCache) &&
                    specializedTypeCache.TryGetValue(arguments, out var specializedType))
                {
                    return specializedType;
                }
            }
            finally
            {
                s_typeCacheLocker.ExitReadLock();
            }

            s_typeCacheLocker.EnterWriteLock();
            try
            {
                IMultiKeyDictionary<Type, Type>? specializedTypeCache;

                if (!s_typeCache.TryGetValue(originalType, out specializedTypeCache))
                {
                    specializedTypeCache = new MultiKeyDictionary<Type, Type>(originalType.GenericTypeArguments.Length);
                    s_typeCache.Add(originalType, specializedTypeCache);
                }

                if (specializedTypeCache.TryGetValue(arguments, out var specializedType))
                    return specializedType;

                var newSpecializedType = originalType.MakeGenericType(arguments is Type[] types ? types : arguments.ToArray());
                specializedTypeCache.Add(arguments, newSpecializedType);

                return newSpecializedType;
            }
            finally
            {
                s_typeCacheLocker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Makes the generic methods from arguments with cache result.
        /// </summary>
        public static MethodInfo MakeGenericMethodWithCache(this MethodInfo genericMethodInfo, params Type[] arguments)
        {
            return MakeGenericMethodWithCache(genericMethodInfo, (IReadOnlyCollection<Type>)arguments);
        }

        /// <summary>
        /// Makes the generic methods from arguments with cache result.
        /// </summary>
        public static MethodInfo MakeGenericMethodWithCache(this MethodInfo genericMethodInfo, IReadOnlyCollection<Type> arguments)
        {
            s_methodCacheLocker.EnterReadLock();
            try
            {
                if (s_methodCache.TryGetValue(genericMethodInfo, out var specialized) && specialized.TryGetValue(arguments, out var specializedMethod))
                    return specializedMethod;
            }
            finally
            {
                s_methodCacheLocker.ExitReadLock();
            }

            s_methodCacheLocker.EnterWriteLock();
            try
            {
                if (s_methodCache.TryGetValue(genericMethodInfo, out var specialized) && specialized.TryGetValue(arguments, out var specializedMethod))
                    return specializedMethod;

                var newSpecializedMethod = genericMethodInfo.MakeGenericMethod(arguments is Type[] types ? types : arguments.ToArray());

                IMultiKeyDictionary<Type, MethodInfo>? speCache;

                if (!s_methodCache.TryGetValue(genericMethodInfo, out speCache))
                {
                    speCache = new MultiKeyDictionary<Type, MethodInfo>(genericMethodInfo.GetGenericArguments().Length);
                    s_methodCache.Add(genericMethodInfo, speCache);
                }

                speCache.Add(arguments, newSpecializedMethod);

                return newSpecializedMethod;
            }
            finally
            {
                s_methodCacheLocker.ExitWriteLock();
            }
        }

        #endregion
    }
}
