// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.Models.Converters
{
    using MARI.Toolbox.Abstractions.Models;

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Convert <see cref="Guid"/> to <see cref="string"/> and revert
    /// </summary>
    /// <seealso cref="IDedicatedObjectConverter" />
    public sealed class GuidDedicatedConverter : IDedicatedObjectConverter
    {
        #region Fields

        private static readonly Type[] s_managedSourceType;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="GuidDedicatedConverter"/> class.
        /// </summary>
        static GuidDedicatedConverter()
        {
            s_managedSourceType = new[] { typeof(Guid), typeof(string) };
        }

        #endregion

        #region Properties

        /// <inheritdoc />
        public IReadOnlyCollection<Type> ManagedSourceTypes
        {
            get { return s_managedSourceType; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool TryConvert(object obj, Type targetType, out object? result)
        {
            if (obj is string str && targetType == typeof(Guid) && Guid.TryParse(str, out var resultGuid))
            {
                result = resultGuid;
                return true;
            }

            if (obj is Guid gObj && targetType == typeof(string))
            {
                result = gObj.ToString();
                return true;
            }

            if (obj is string strObj && targetType == typeof(string))
            {
                result = strObj;
                return true;
            }

            if (obj is Guid gObjToG && targetType == typeof(Guid))
            {
                result = gObjToG;
                return true;
            }

            result = null;
            return false;
        }

        #endregion
    }
}
