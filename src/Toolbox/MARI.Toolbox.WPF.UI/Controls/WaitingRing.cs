// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Infinit waiting ring
    /// </summary>
    /// <seealso cref="System.Windows.Controls.Control" />
    public class WaitingRing : Control
    {
        #region Fields

        public static readonly DependencyProperty CircleSizeProperty = DependencyProperty.Register(nameof(CircleSize),
                                                                                                   typeof(double),
                                                                                                   typeof(WaitingRing),
                                                                                                   new PropertyMetadata(10.0d));

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="WaitingRing"/> class.
        /// </summary>
        static WaitingRing()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WaitingRing), new FrameworkPropertyMetadata(typeof(WaitingRing)));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the size of the circle.
        /// </summary>
        public double CircleSize
        {
            get { return (double)GetValue(CircleSizeProperty); }
            set { SetValue(CircleSizeProperty, value); }
        }

        #endregion

        #region Method

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var minSize = Math.Min(arrangeBounds.Width, arrangeBounds.Height);
            this.CircleSize = minSize / (5 * 2);
            return base.ArrangeOverride(new Size(minSize, minSize));
        }

        #endregion
    }
}
