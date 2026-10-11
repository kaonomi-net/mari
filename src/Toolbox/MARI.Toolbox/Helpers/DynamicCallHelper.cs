// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Helpers
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Helper used to perform runtime dynamic call
    /// </summary>
    public static class DynamicCallHelper
    {
        #region Fields

        private static readonly Regex s_indexProp = new Regex(@"^(?<prop>[a-zA-Z]+)\[(?<index>.*)\]$", RegexOptions.Compiled);

        #endregion

        #region Methods

        /// <summary>
        /// Extract simple call chain from expression
        /// </summary>
        public static string? GetCallChain<TInput, TResult>(Expression<Func<TInput, TResult>>? expression)
        {
            return GetCallChain(expression?.Body);
        }

        /// <summary>
        /// Gets the call chain.
        /// </summary>
        public static string? GetCallChain(Expression? expression = null)
        {
            if (expression is null)
                return null;

            var chain = new StringBuilder(42);

            var currentChainPartExpression = expression;

            while (currentChainPartExpression != null)
            {
                if (currentChainPartExpression is MemberExpression memberAccessExpression)
                {
                    chain.Insert(0, memberAccessExpression.Member.Name);

                    if (memberAccessExpression.Expression != null)
                        chain.Insert(0, ".");

                    currentChainPartExpression = memberAccessExpression.Expression;
                }
                else if (currentChainPartExpression is ParameterExpression parameter)
                {
                    chain.Insert(0, parameter.Name);
                    break;
                }
                else if (currentChainPartExpression is MethodCallExpression method &&
                         method.Method.IsSpecialName &&
                         method.Method.Name == "get_Item" &&
                         method.Arguments.Count == 1 &&
                         method.Arguments[0] is MemberExpression arg)
                {
                    var val = ExpressionExtensions.ExtractConstantValue(arg, out var type);
                    chain.Insert(0, "[" + val + "]");

                    currentChainPartExpression = method.Object;
                }
                else
                {
                    throw new NotSupportedException("Only member acces chain is tolerate. Expression :" + expression + ", Failure part :" + currentChainPartExpression);
                }
            }

            return chain.ToString();
        }

        /// <summary>
        /// Compiles the call chain access expression
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LambdaExpression CompileCallChainAccess<TInput>(string callChain, bool containRoot = false, bool throwIfNotFounded = true)
        {
            return CompileCallChainAccess(typeof(TInput), callChain, containRoot, throwIfNotFounded);
        }

        /// <summary>
        /// Compiles the call chain access expression
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LambdaExpression CompileCallChainAccess(Type input, string callChain, bool containRoot = false, bool throwIfNotFounded = true)
        {
            return CompileCallChainAccess(Expression.Parameter(input), callChain, containRoot, throwIfNotFounded);
        }

        /// <summary>
        /// Compiles the call chain access expression
        /// </summary>
        public static LambdaExpression CompileCallChainAccess(ParameterExpression parameterExpression, string callChain, bool containRoot = false, bool throwIfNotFounded = true)
        {
            ReadOnlySpan<char> callChainsSpan = callChain;
            return CompileCallChainAccess(parameterExpression, callChainsSpan, containRoot, throwIfNotFounded);
        }

        /// <summary>
        /// Compiles the call chain access expression
        /// </summary>
        public static LambdaExpression CompileCallChainAccess(ParameterExpression parameterExpression, ReadOnlySpan<char> callChain, bool containRoot = false, bool throwIfNotFounded = true)
        {
            var accessExpression = CompileCallChainAccessImpl(parameterExpression, callChain, containRoot, throwIfNotFounded) ?? parameterExpression;
            return Expression.Lambda(accessExpression, parameterExpression);
        }

        /// <summary>
        /// Compiles the call chain access expression
        /// </summary>
        private static Expression? CompileCallChainAccessImpl(Expression parameterExpression, ReadOnlySpan<char> callChain, bool containRoot = false, bool throwIfNotFounded = true)
        {
            if (parameterExpression is null)
            {
                if (throwIfNotFounded)
                    throw new ArgumentNullException(nameof(parameterExpression));

                return null;
            }

            var trait = parameterExpression.Type;

            var prop = callChain;
            ReadOnlySpan<char> tail = default;

            var dotIndexOf = callChain.IndexOf('.');

            if (dotIndexOf > -1)
            {
                prop = callChain.Slice(0, dotIndexOf);
                tail = callChain.Slice(dotIndexOf + 1);
            }

            if (containRoot)
            {
                if (dotIndexOf > -1)
                    return CompileCallChainAccessImpl(parameterExpression, tail, containRoot: false, throwIfNotFounded);

                return parameterExpression;
            }

            var indexProp = string.Empty;
            var match = s_indexProp.Match(prop.ToString());

            if (match.Success)
            {
                var grpMatch = match.Groups["prop"];

                if (grpMatch.Success)
                    prop = grpMatch.Value;

                var grpMatchIndx = match.Groups["index"];
                if (grpMatchIndx.Success)
                    indexProp = grpMatchIndx.Value;
            }

            PropertyInfo? info = null;

            foreach (var property in trait.GetAllPropertyInfos(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.Equals(property.Name, StringComparison.OrdinalIgnoreCase))
                {
                    info = property;
                    break;
                }
            }

            if (info == null && throwIfNotFounded)
                throw new ArgumentException("Missing or null call chain  part '" + prop.ToString() + "'");

            Expression resolvedValue = Expression.Property(parameterExpression, info!);

            if (!string.IsNullOrEmpty(indexProp))
            {
                var mthd = info!.PropertyType.GetMethod("get_Item");
                if (mthd is not null && mthd.IsSpecialName)
                {
                    var convertValue = ConvertHelper.ConvertValueFromString(mthd.GetParameters()[0].ParameterType, indexProp);
                    resolvedValue = Expression.Call(resolvedValue, mthd, Expression.Constant(convertValue));
                }
            }

            if (tail.Length > 0 && resolvedValue is not null)
                return CompileCallChainAccessImpl(resolvedValue, tail, containRoot: false, throwIfNotFounded);

            return resolvedValue!;
        }

        /// <summary>
        /// Helper able to get a value in a object tree using the calling path.
        /// </summary>
        /// <remarks>
        ///     Use reflection without caching
        /// </remarks>
        public static object? GetValueFrom(object? inst, string callChain, bool containRoot = false, bool throwIfNotFounded = true)
        {
            ReadOnlySpan<char> callChainsSpan = callChain;
            return GetValueFrom(inst, callChainsSpan, containRoot, throwIfNotFounded);
        }

        /// <summary>
        /// Helper able to get a value in a object tree using the calling path.
        /// </summary>
        /// <remarks>
        ///     Use reflection without caching
        /// </remarks>
        public static object? GetValueFrom(object? inst, ReadOnlySpan<char> callChain, bool containRoot = false, bool throwIfNotFounded = true)
        {
            if (inst is null)
            {
                if (throwIfNotFounded)
                    throw new ArgumentNullException(nameof(inst));

                return null;
            }

            var trait = inst.GetType();

            var prop = callChain;
            ReadOnlySpan<char> tail = default;

            var dotIndexOf = callChain.IndexOf('.');

            if (dotIndexOf > -1)
            {
                prop = callChain.Slice(0, dotIndexOf);
                tail = callChain.Slice(dotIndexOf + 1);
            }

            if (containRoot)
            {
                if (dotIndexOf > -1)
                    return GetValueFrom(inst, tail, containRoot: false, throwIfNotFounded);

                return inst;
            }

            var indexProp = string.Empty;
            var match = s_indexProp.Match(prop.ToString());

            if (match.Success)
            {
                var grpMatch = match.Groups["prop"];

                if (grpMatch.Success)
                    prop = grpMatch.Value;

                var grpMatchIndx = match.Groups["index"];
                if (grpMatchIndx.Success)
                    indexProp = grpMatchIndx.Value;
            }

            PropertyInfo? info = null;

            foreach (var property in trait.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.Equals(property.Name, StringComparison.OrdinalIgnoreCase))
                {
                    info = property;
                    break;
                }
            }

            if (info == null && throwIfNotFounded)
                throw new ArgumentException("Missing or null call chain  part '" + prop.ToString() + "'");

            var resolvedValue = info?.GetValue(inst, null);

            if (!string.IsNullOrEmpty(indexProp) && resolvedValue is not null)
            {
                var type = resolvedValue.GetType();

                var mthd = type.GetMethod("get_Item");
                if (mthd is not null)
                {
                    var convertValue = ConvertHelper.ConvertValueFromString(mthd.GetParameters()[0].ParameterType, indexProp);
                    resolvedValue = mthd.Invoke(resolvedValue, new[] { convertValue });
                }
            }

            if (tail.Length > 0 && resolvedValue is not null)
                return GetValueFrom(resolvedValue, tail, containRoot: false, throwIfNotFounded);

            return resolvedValue;
        }

        #endregion
    }
}