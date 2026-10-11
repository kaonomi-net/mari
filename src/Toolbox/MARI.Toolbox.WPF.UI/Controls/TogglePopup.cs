namespace MARI.Toolbox.WPF.UI.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;

    /// <summary>
    /// Popup with a toggle button as trigger
    /// </summary>
    /// <seealso cref="HeaderedContentControl" />
    [TemplatePart(Name = PART_Popup, Type = typeof(Popup))]
    public sealed class TogglePopup : HeaderedContentControl
    {
        #region Fields

        public const string PART_Popup = "PART_Popup";

        public static readonly DependencyProperty IsCheckedProperty = ToggleButton.IsCheckedProperty.AddOwner(typeof(TogglePopup),
                                                                                                              new FrameworkPropertyMetadata(OnCheckPropertyChanged));
        private Popup? _popup;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="TogglePopup"/> class.
        /// </summary>
        static TogglePopup()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TogglePopup), new FrameworkPropertyMetadata(typeof(TogglePopup)));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is checked.
        /// </summary>
        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            this._popup = base.GetTemplateChild(PART_Popup) as Popup;
            base.OnApplyTemplate();
        }

        /// <summary>
        /// Called when [check property changed].
        /// </summary>
        private static void OnCheckPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TogglePopup popup)
                popup.OnCheckChanged((bool)e.NewValue);
        }

        /// <summary>
        /// Called when [check changed].
        /// </summary>
        private void OnCheckChanged(bool newValue)
        {
            var root = this.GetTreeRoot();
            if (newValue)
                Mouse.AddPreviewMouseDownHandler(root, OnMouseButtonEvent);
            else
                Mouse.RemovePreviewMouseDownHandler(root, OnMouseButtonEvent);
        }

        /// <summary>
        /// Called when to see if the popup must be closed
        /// </summary>
        private void OnMouseButtonEvent(object sender, MouseButtonEventArgs e)
        {
            if (this.IsChecked == false || this._popup is null)
                return;

            this.IsChecked = this._popup.Child is null ||
                             (e.OriginalSource is Visual  dpoSource && this._popup.Child.IsAncestorOf(dpoSource));
        }

        #endregion
    }
}
