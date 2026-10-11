// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Models.Converters
{
    using MARI.Toolbox.Abstractions.Models;

    using System;
    using System.Collections.Frozen;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Converter use to transform scalar c# type one to other if possible using <see cref="Convert"/> object
    /// </summary>
    /// <seealso cref="IDedicatedObjectConverter" />
    public sealed class ScalarDedicatedConverter : IDedicatedObjectConverter
    {
        #region Fields

        // KEY => Tuple<Type, Type> == <INPUT, EXPECTED>
        private static readonly IReadOnlyDictionary<Tuple<Type, Type>, MethodInfo> s_convertMethod;
        private static readonly IReadOnlySet<Type> s_managedSourceTypes;

        private static readonly Type s_stringType;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="ScalarDedicatedConverter"/> class.
        /// </summary>
        static ScalarDedicatedConverter()
        {
            s_stringType = typeof(string);
            s_managedSourceTypes = CSharpTypeInfo.ScalarTypes;

            var convertMethod = typeof(Convert).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                               .Where(m => m.ReturnType != typeof(object) &&
                                                           s_managedSourceTypes.Contains(m.ReturnType) &&
                                                           m.GetParameters().Length == 1 &&
                                                           m.Name == "To" + m.ReturnType.Name &&
                                                           m.GetParameters().First().ParameterType != typeof(object) &&
                                                           s_managedSourceTypes.Contains(m.GetParameters().First().ParameterType))
                                               .GroupBy(m => (Param: m.GetParameters().First(), Return: m.ReturnType))
                                               .ToFrozenDictionary(k => Tuple.Create(k.Key.Param.ParameterType, k.Key.Return), k => k.OrderByDescending(m => m.Name.Length).First());

            s_convertMethod = convertMethod;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public IReadOnlyCollection<Type> ManagedSourceTypes
        {
            get { return s_managedSourceTypes; }
        }

        /// <inheritdoc/>
        public bool TryConvert(object obj, Type targetType, out object? result)
        {
            result = null;
            var objType = obj?.GetType();
            try
            {
                if (targetType == s_stringType)
                {
                    result = obj.ToString();
                    return true;
                }

                if (objType is not null && s_convertMethod.TryGetValue(Tuple.Create(objType, targetType), out var convMethod))
                {
                    result = convMethod.Invoke(null, new[] { obj });
                    return true;
                }
            }
            catch (FormatException)
            {
            }
            return false;
        }

        #endregion
    }
}
