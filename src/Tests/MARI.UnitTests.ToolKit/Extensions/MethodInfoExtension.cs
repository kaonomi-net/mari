// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.UnitTests.ToolKit.Extensions
{
    using System.Linq;
    using System.Reflection;

    internal static class MethodInfoExtension
    {
        /// <summary>
        /// Placeholder type used to close a generic method when no interface constraint could be used
        /// </summary>
        public sealed class AnyType
        {
        }

        /// <summary>
        /// Gets the default generic implementation method.
        /// </summary>
        /// <remarks>
        ///     Resolve Generic defintion by constrains or by <see cref="AnyType"/>
        /// </remarks>
        public static MethodInfo GetDefaultGenericImplementationMethod(this MethodInfo method)
        {
            if (method.IsGenericMethod)
            {
                var defaultGenericTypes = method.GetGenericArguments()
                                                .Select(t => t.GetGenericParameterConstraints().FirstOrDefault(t => t.IsInterface) ?? typeof(AnyType))
                                                .ToArray();

                method = method.MakeGenericMethod(defaultGenericTypes);
            }

            return method;
        }
    }
}
