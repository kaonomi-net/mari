// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Framework.Core.Abstractions.Attributes
{
    using MARI.Framework.Core.Abstractions.Enums;

    using System;

    /// <summary>
    /// Base attribute about how vgrain id must be formated
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class VGrainIdBaseFormatorAttribute : Attribute
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="VGrainIdFormatAttribute"/> class.
        /// </summary>
        protected VGrainIdBaseFormatorAttribute(IdFormatTypeEnum formatType)
        {
            this.FormatType = formatType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of the format.
        /// </summary>
        public IdFormatTypeEnum FormatType { get; init; }

        #endregion

        #region Methods
        #endregion
    }
}
