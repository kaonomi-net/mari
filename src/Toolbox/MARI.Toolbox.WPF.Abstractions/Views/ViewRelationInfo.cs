// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.Abstractions.Views
{
    /// <summary>
    /// Define view position information
    /// </summary>
    public class ViewRelationInfo
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewRelationInfo"/> class.
        /// </summary>
        public ViewRelationInfo(string? key = null,
                                ViewPositionEnum? viewPosition = ViewPositionEnum.None,
                                string? customViewPositionLabel = null,
                                bool? onlyOneAtTime = false,
                                bool? persistant = false)
        {
            this.Key = key;
            this.ViewPosition = viewPosition;
            this.CustomViewPositionLabel = customViewPositionLabel;
            this.OnlyOneAtTime = onlyOneAtTime;
            this.Persistant = persistant;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the key.
        /// </summary>
        public string? Key { get; }

        /// <summary>
        /// Gets the view position.
        /// </summary>
        public ViewPositionEnum? ViewPosition { get; }

        /// <summary>
        /// Gets the custom view position label.
        /// </summary>
        public string? CustomViewPositionLabel { get; }

        /// <summary>
        /// Gets the only one at time.
        /// </summary>
        public bool? OnlyOneAtTime { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ViewRelationInfo"/> is persistant.
        /// </summary>
        public bool? Persistant { get; }

        #endregion
    }
}
