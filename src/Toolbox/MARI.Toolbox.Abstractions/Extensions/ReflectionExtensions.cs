// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace System.Reflection
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Extensions methods about reflection tools
    /// </summary>
    public static class ReflectionExtensions
    {
        #region Fields

        private static readonly Dictionary<MethodBase, string> s_methodUniqueId;
        private static readonly Dictionary<string, string> s_methodUniqueIdOwners;
        private static readonly ConcurrentDictionary<Type, HashSet<string>> s_ambiguousDisplayNames;

        private static readonly ReaderWriterLockSlim s_methodUniqueIdLocker;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ReflectionExtensions"/> class.
        /// </summary>
        static ReflectionExtensions()
        {
            s_methodUniqueIdLocker = new ReaderWriterLockSlim();
            s_methodUniqueId = new Dictionary<MethodBase, string>();
            s_methodUniqueIdOwners = new Dictionary<string, string>(StringComparer.Ordinal);
            s_ambiguousDisplayNames = new ConcurrentDictionary<Type, HashSet<string>>();
        }

        #endregion

        #region Method

        /// <summary>
        /// Gets the unique identifier.
        /// </summary>
        /// <remarks>
        ///     This unique ID is only guarenty unique by <see cref="AppDomain"/>
        ///     We use TypeFullName + MethodFUllName hash by MD5.
        ///     If several methods share the same display name the fully qualified signature is introduced in the hash to keep the id unique and deterministic
        /// </remarks>
        public static string GetUniqueId(this MethodBase methodInfo, bool raisedErrorIfDuplicate = false, bool useCache = true)
        {
            if (useCache)
            {
                s_methodUniqueIdLocker.EnterReadLock();

                try
                {
                    if (s_methodUniqueId.TryGetValue(methodInfo, out var uniqueKey))
                        return uniqueKey;
                }
                finally
                {
                    s_methodUniqueIdLocker.ExitReadLock();
                }
            }

            // Deterministic id :
            //  - Default id is based on the method display name (stable, unchanged from previous versions)
            //  - If the display name is shared by several methods of the same declaring type
            //    the fully qualified signature is mixed into the hash (order independent)
            var fullSignature = BuildMethodFullSignature(methodInfo);
            var hashKey = IsDisplayNameAmbiguous(methodInfo)
                                ? BuildMethodUniqueId(methodInfo, fullSignature)
                                : BuildMethodUniqueId(methodInfo, null);

            if (useCache == false)
                return hashKey;

            s_methodUniqueIdLocker.EnterWriteLock();

            try
            {
                if (s_methodUniqueId.TryGetValue(methodInfo, out var uniqueKey))
                    return uniqueKey;

                if (s_methodUniqueIdOwners.TryGetValue(hashKey, out var owner) && !string.Equals(owner, fullSignature, StringComparison.Ordinal))
                {
                    if (raisedErrorIfDuplicate)
                        throw new InvalidOperationException("Could not genenerate unique id for " + methodInfo + " because another method have the same hash =>" + owner);

                    // Collision with a method of another type sharing the same short name, use the deterministic fully qualified signature id
                    hashKey = BuildMethodUniqueId(methodInfo, fullSignature);

                    if (s_methodUniqueIdOwners.TryGetValue(hashKey, out owner) && !string.Equals(owner, fullSignature, StringComparison.Ordinal))
                        throw new InvalidOperationException("Could not genenerate unique id for " + methodInfo + " because another method have the same hash =>" + owner);
                }

                s_methodUniqueId.Add(methodInfo, hashKey);
                s_methodUniqueIdOwners.TryAdd(hashKey, fullSignature);

                return hashKey;
            }
            finally
            {
                s_methodUniqueIdLocker.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public static string GetDisplayName(this MethodBase methodInfo)
        {
            var keyBuilder = new StringBuilder();

            keyBuilder.Append("[");
            keyBuilder.Append(methodInfo.DeclaringType?.GetTypeInfoExtension().FullShortName);
            keyBuilder.Append("] ");

            BuildMethodDisplayName(methodInfo, keyBuilder);

            return keyBuilder.ToString();
        }

        #region Tools

        /// <summary>
        /// Builds a display name for method <paramref name="methodInfo"/>
        /// </summary>
        private static void BuildMethodDisplayName(this MethodBase methodInfo, StringBuilder keyBuilder)
        {
            if (methodInfo is MethodInfo mInfo)
            {
                keyBuilder.Append(mInfo.ReturnType.FullName);
                keyBuilder.Append(" ");
            }

            keyBuilder.Append(methodInfo.Name);

            if (methodInfo.IsGenericMethod)
            {
                keyBuilder.Append("<");

                bool first = true;
                foreach (var geneParam in methodInfo.GetGenericArguments())
                {
                    if (!first)
                        keyBuilder.Append(",");

                    first = false;
                    keyBuilder.Append(geneParam.GetAbstractType().DisplayName);
                }

                keyBuilder.Append(">");
            }

            keyBuilder.Append("(");

            bool firstParams = true;
            foreach (var param in methodInfo.GetParameters())
            {
                if (!firstParams)
                    keyBuilder.Append(",");

                firstParams = false;
                keyBuilder.Append(param.ParameterType.GetAbstractType().DisplayName);
            }

            keyBuilder.Append(")");
        }

        /// <summary>
        /// Builds the method unique identifier.
        /// </summary>
        private static string BuildMethodUniqueId(MethodBase methodInfo, string? disambiguator)
        {
            var keyBuilder = new StringBuilder();

            BuildMethodDisplayName(methodInfo, keyBuilder);

            if (disambiguator is not null)
            {
                keyBuilder.Append(':');
                keyBuilder.Append(disambiguator);
            }

            using (var hash = SHA512.Create())
            {
                var keyBytes = Encoding.UTF8.GetBytes(keyBuilder.ToString());
                var hashKeyBytes = hash.ComputeHash(keyBytes);

                keyBuilder.Clear();

                var shortName = methodInfo.DeclaringType?.GetTypeInfoExtension().FullShortName ?? string.Empty;
                keyBuilder.EnsureCapacity((keyBytes.Length * 2) + shortName.Length + 1);

                keyBuilder.Append(shortName);
                keyBuilder.Append("_");
                keyBuilder.Append(Convert.ToBase64String(hashKeyBytes));

                return keyBuilder.ToString();
            }
        }

        /// <summary>
        /// Gets the display key used to build the default unique id
        /// </summary>
        private static string GetMethodDisplayKey(MethodBase methodInfo)
        {
            var keyBuilder = new StringBuilder();
            BuildMethodDisplayName(methodInfo, keyBuilder);
            return keyBuilder.ToString();
        }

        /// <summary>
        /// Determines whether the display key of <paramref name="methodInfo"/> is shared by several methods of its declaring type.
        /// </summary>
        /// <remarks>
        ///     Only depends on the declaring type, so the result is the same whatever the call order or the process.
        /// </remarks>
        private static bool IsDisplayNameAmbiguous(MethodBase methodInfo)
        {
            var declaringType = methodInfo.DeclaringType;
            if (declaringType is null)
                return false;

            var reference = methodInfo;
            if (methodInfo is MethodInfo mInfo && mInfo.IsGenericMethod && !mInfo.IsGenericMethodDefinition)
                reference = mInfo.GetGenericMethodDefinition();

            var ambiguous = s_ambiguousDisplayNames.GetOrAdd(declaringType, BuildAmbiguousDisplayNames);
            if (ambiguous.Count == 0)
                return false;

            return ambiguous.Contains(GetMethodDisplayKey(reference));
        }

        /// <summary>
        /// Builds the display keys shared by several methods/constructors declared by <paramref name="type"/>
        /// </summary>
        private static HashSet<string> BuildAmbiguousDisplayNames(Type type)
        {
            const BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            var keys = new HashSet<string>(StringComparer.Ordinal);
            var ambiguous = new HashSet<string>(StringComparer.Ordinal);

            IEnumerable<MethodBase> members;
            try
            {
                members = type.GetMethods(flags).Cast<MethodBase>().Concat(type.GetConstructors(flags)).ToArray();
            }
            catch
            {
                return ambiguous;
            }

            foreach (var member in members)
            {
                string key;
                try
                {
                    key = GetMethodDisplayKey(member);
                }
                catch
                {
                    continue;
                }

                if (!keys.Add(key))
                    ambiguous.Add(key);
            }

            return ambiguous;
        }

        /// <summary>
        /// Builds a fully qualified signature (declaring type, name, generic arity/arguments, parameters and return types)
        /// independent of assembly versions and stable between processes.
        /// </summary>
        private static string BuildMethodFullSignature(MethodBase methodInfo)
        {
            var builder = new StringBuilder();

            if (methodInfo.DeclaringType is not null)
                AppendFullTypeId(methodInfo.DeclaringType, builder);

            builder.Append("::");
            builder.Append(methodInfo.Name);

            if (methodInfo.IsGenericMethod)
            {
                var genericArgs = methodInfo.GetGenericArguments();
                builder.Append('`');
                builder.Append(genericArgs.Length);

                if (!methodInfo.IsGenericMethodDefinition)
                {
                    builder.Append('[');
                    for (var i = 0; i < genericArgs.Length; i++)
                    {
                        if (i > 0)
                            builder.Append(',');
                        AppendFullTypeId(genericArgs[i], builder);
                    }
                    builder.Append(']');
                }
            }

            builder.Append('(');

            var parameters = methodInfo.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                if (i > 0)
                    builder.Append(',');
                AppendFullTypeId(parameters[i].ParameterType, builder);
            }

            builder.Append(')');

            if (methodInfo is MethodInfo mInfo)
            {
                builder.Append("->");
                AppendFullTypeId(mInfo.ReturnType, builder);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Appends a fully qualified type identity (full name + assembly simple name, generic parameters by position)
        /// </summary>
        private static void AppendFullTypeId(Type type, StringBuilder builder)
        {
            if (type.IsGenericMethodParameter)
            {
                builder.Append("!!");
                builder.Append(type.GenericParameterPosition);
                return;
            }

            if (type.IsGenericTypeParameter)
            {
                builder.Append('!');
                builder.Append(type.GenericParameterPosition);
                return;
            }

            if (type.HasElementType)
            {
                AppendFullTypeId(type.GetElementType()!, builder);

                if (type.IsByRef)
                    builder.Append('&');
                else if (type.IsPointer)
                    builder.Append('*');
                else if (type.IsArray)
                {
                    builder.Append('[');
                    builder.Append(type.IsSZArray ? "" : type.GetArrayRank().ToString(Globalization.CultureInfo.InvariantCulture));
                    builder.Append(']');
                }

                return;
            }

            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                AppendFullTypeId(type.GetGenericTypeDefinition(), builder);

                builder.Append('[');
                var args = type.GetGenericArguments();
                for (var i = 0; i < args.Length; i++)
                {
                    if (i > 0)
                        builder.Append(',');
                    AppendFullTypeId(args[i], builder);
                }
                builder.Append(']');
                return;
            }

            builder.Append('{');
            builder.Append(type.FullName ?? ((type.Namespace is null ? "" : type.Namespace + ".") + type.Name));
            builder.Append(", ");
            builder.Append(type.Assembly.GetName().Name);
            builder.Append('}');
        }

        #endregion

        #endregion
    }
}
