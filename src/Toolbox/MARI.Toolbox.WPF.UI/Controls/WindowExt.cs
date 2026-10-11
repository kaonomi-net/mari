// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Controls
{
    using MARI.Toolbox.WPF.Commands;

    using System;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// Window extension
    /// </summary>
    /// <seealso cref="Window" />
    public class WindowExt : Window
    {
        #region Fields

        private static readonly DependencyPropertyKey s_closeCommandPropertyKey = DependencyProperty.RegisterReadOnly(nameof(CloseCommand),
                                                                                                                      typeof(ICommand),
                                                                                                                      typeof(WindowExt),
                                                                                                                      new PropertyMetadata(null));

        public static readonly DependencyProperty CloseCommandProperty = s_closeCommandPropertyKey.DependencyProperty;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowExt"/> class.
        /// </summary>
        public WindowExt()
        {
            SetValue(s_closeCommandPropertyKey, new DelegateCommand(() => Close()));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the close command.
        /// </summary>
        public ICommand CloseCommand
        {
            get { return (ICommand)GetValue(CloseCommandProperty); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="FrameworkElement.DataContext"/> is disposed when the window is closed.
        /// </summary>
        /// <remarks>
        ///     Set to <c>false</c> by view managers that own the view model lifetime (persistent view models)
        /// </remarks>
        public bool DisposeDataContextOnClosed { get; set; } = true;

        #endregion

        #region Methods

        /// <summary>
        /// Raises the <see cref="E:Closed" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            if (this.DisposeDataContextOnClosed && this.DataContext is IDisposable disposable)
                disposable.Dispose();

            base.OnClosed(e);
        }

        #endregion
    }
}
