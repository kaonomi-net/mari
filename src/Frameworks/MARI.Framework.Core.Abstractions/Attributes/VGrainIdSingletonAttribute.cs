// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Attributes
{
    using MARI.Framework.Core.Abstractions.Enums;

    using System;

    /// <summary>
    /// Specific how vgrain id must be formated
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public sealed class VGrainIdSingletonAttribute : VGrainIdBaseFormatorAttribute
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainIdFormatAttribute"/> class.
        /// </summary>
        public VGrainIdSingletonAttribute(string? singletonValue = null, IdFormatTypeEnum type = IdFormatTypeEnum.String)
            : base(type)
        {
            this.SingletonValue = singletonValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the singleton value.
        /// </summary>
        public string? SingletonValue { get; }

        #endregion
    }
}
