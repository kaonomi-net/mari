// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Controls
{
    using MARI.Toolbox.WPF.Commands;
    using MARI.Toolbox.WPF.UI.Exceptions;

    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    /// <summary>
    /// Define a window without border and without distinction between top bar an menu
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    public class BorderLessWindow : WindowExt
    {
        #region Fields

        public const string PART_HEADERBAR = "PART_HeaderBar";

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header),
                                                                                               typeof(object),
                                                                                               typeof(BorderLessWindow),
                                                                                               new PropertyMetadata(null));

        public static readonly DependencyProperty HeaderBackgroundProperty = DependencyProperty.Register(nameof(HeaderBackground),
                                                                                                         typeof(Brush),
                                                                                                         typeof(BorderLessWindow),
                                                                                                         new PropertyMetadata(null));


        private static readonly DependencyPropertyKey s_minimizeCommandPropertyKey = DependencyProperty.RegisterReadOnly(nameof(MinimizeCommand),
                                                                                                                         typeof(ICommand),
                                                                                                                         typeof(BorderLessWindow),
                                                                                                                         new PropertyMetadata(null));

        public static readonly DependencyProperty MinimizeCommandProperty = s_minimizeCommandPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey s_toggleMaximizeReduceCommandPropertyKey = DependencyProperty.RegisterReadOnly(nameof(ToggleMaximizeReduceCommand),
                                                                                                                                     typeof(ICommand),
                                                                                                                                     typeof(BorderLessWindow),
                                                                                                                                     new PropertyMetadata(null));

        public static readonly DependencyProperty ToggleMaximizeReduceCommandProperty = s_toggleMaximizeReduceCommandPropertyKey.DependencyProperty;

        private FrameworkElement? _dragArea;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="BorderLessWindow"/> class.
        /// </summary>
        static BorderLessWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BorderLessWindow), new FrameworkPropertyMetadata(typeof(BorderLessWindow)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BorderLessWindow"/> class.
        /// </summary>
        public BorderLessWindow()
        {
            SetValue(s_minimizeCommandPropertyKey, new DelegateCommand(() => this.WindowState = WindowState.Minimized));
            SetValue(s_toggleMaximizeReduceCommandPropertyKey, new DelegateCommand(() => this.WindowState = (this.WindowState != WindowState.Maximized) ? WindowState.Maximized : WindowState.Normal));
        }

        #endregion

        #region Properties

        #region Commands

        /// <summary>
        /// Gets the minimize command.
        /// </summary>
        public ICommand MinimizeCommand
        {
            get { return (ICommand)GetValue(MinimizeCommandProperty); }
        }

        /// <summary>
        /// Gets the toggle maximize/reduce command.
        /// </summary>
        public ICommand ToggleMaximizeReduceCommand
        {
            get { return (ICommand)GetValue(ToggleMaximizeReduceCommandProperty); }
        }

        #endregion

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        /// <summary>
        /// Gets or sets the header background.
        /// </summary>
        public Brush HeaderBackground
        {
            get { return (Brush)GetValue(HeaderBackgroundProperty); }
            set { SetValue(HeaderBackgroundProperty, value); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.
        /// </summary>
        public override void OnApplyTemplate()
        {
            var dragArea = GetTemplateChild(PART_HEADERBAR) as FrameworkElement ?? throw new TemplatePartMissingException(typeof(FrameworkElement), PART_HEADERBAR, this);

            dragArea.MouseLeftButtonDown -= DragArea_MouseLeftButtonDown;
            dragArea.MouseLeftButtonDown += DragArea_MouseLeftButtonDown;

            this._dragArea = dragArea;
            base.OnApplyTemplate();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Controls.Control.MouseDoubleClick" /> routed event.
        /// </summary>
        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (this._dragArea is null)
                return;

            var hit = this._dragArea.InputHitTest(e.GetPosition(this._dragArea));

            if (this._dragArea == hit)
                this.ToggleMaximizeReduceCommand.Execute(null);

            base.OnMouseDoubleClick(e);
        }

        /// <summary>
        /// Handles the MouseLeftButtonDown event of the DragArea control.
        /// </summary>
        private void DragArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        #endregion
    }
}
