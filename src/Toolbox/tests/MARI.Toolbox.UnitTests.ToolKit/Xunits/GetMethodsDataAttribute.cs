// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.UnitTests.ToolKit.Xunits
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Xunit.Sdk;

    /// <summary>
    /// Provide all methods of type <typeparamref name="TType"/> value
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class GetMethodsDataAttribute : DataAttribute
    {
        #region Fields

        private readonly IReadOnlyCollection<object[]> _values;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumDataAttribute{TEnum}"/> class.
        /// </summary>
        public GetMethodsDataAttribute(BindingFlags flags = BindingFlags.Instance | BindingFlags.Public, params Type[] types)
        {
            this._values = types.SelectMany(trait => trait.GetAllMethodInfos(flags).Select(m => (Method: m, Type: trait)))
                                .Where(o => o.Type != typeof(object) && o.Method.DeclaringType != typeof(object))
                                .Distinct()
                                .Select(v => new object[] { v.Method.GetAbstractMethod(useCache: false).DisplayName, v.Type, v.Method.GetAbstractMethod(useCache: false).MethodUniqueId })
                                .ToArray();
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            return this._values;
        }

        #endregion
    }
}
