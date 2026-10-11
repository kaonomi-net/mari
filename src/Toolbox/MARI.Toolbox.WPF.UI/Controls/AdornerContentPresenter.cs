// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;

    /// <summary>
    /// Adorner use to display content
    /// </summary>
    /// <seealso cref="System.Windows.Documents.Adorner" />
    public sealed class AdornerContentPresenter<TAdornerDisplayControl> : Adorner
            where TAdornerDisplayControl : Control
    {
        #region Fields

        private readonly ContentPresenter _contentPresenter;
        private readonly VisualCollection _visuals;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentControlAdorner{TAdornerDisplayControl}"/> class.
        /// </summary>
        public AdornerContentPresenter(UIElement adornedElement)
          : this(adornedElement, Activator.CreateInstance<TAdornerDisplayControl>())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentControlAdorner{TAdornerDisplayControl}"/> class.
        /// </summary>
        public AdornerContentPresenter(UIElement adornedElement, TAdornerDisplayControl content)
          : base(adornedElement)
        {
            this._visuals = new VisualCollection(this);
            this._contentPresenter = new ContentPresenter();
            this._visuals.Add(_contentPresenter);

            this.Content = content;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        public object Content
        {
            get { return this._contentPresenter.Content; }
            set { this._contentPresenter.Content = value; }
        }

        /// <inheritdoc />
        protected override int VisualChildrenCount
        {
            get { return _visuals.Count; }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override Size MeasureOverride(Size constraint)
        {
            this._contentPresenter.Measure(constraint);
            this.AdornedElement.Measure(constraint);
            //return this.AdornedElement.DesiredSize;
            return base.MeasureOverride(constraint);
        }

        /// <inheritdoc />
        protected override Size ArrangeOverride(Size finalSize)
        {
            this._contentPresenter.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            return this._contentPresenter.RenderSize;
        }

        /// <inheritdoc />
        protected override Visual GetVisualChild(int index)
        {
            return this._visuals[index];
        }

        #endregion
    }
}
