namespace MARI.Toolbox.WPF.UI.Services
{
    using MARI.Toolbox.Abstractions.Proxies;
    using MARI.Toolbox.WPF.Abstractions.Navigations;
    using MARI.Toolbox.WPF.Abstractions.Views;
    using MARI.Toolbox.WPF.Services;
    using MARI.Toolbox.WPF.UI.Controls;

    using Microsoft.Extensions.DependencyInjection;

    using System;
    using System.Collections.Generic;
    using System.Windows;

    /// <summary>
    /// Base class implementation of <see cref="IViewManager"/> that manager the window openning
    /// </summary>
    /// <seealso cref="MARI.Toolbox.WPF.Services.BaseViewManager" />
    public abstract class BaseViewWithWindowManager : BaseViewManager
    {
        #region Field

        private static readonly Type s_windowType;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes the <see cref="BaseViewWithWindowManager"/> class.
        /// </summary>
        static BaseViewWithWindowManager()
        {
            s_windowType = typeof(Window);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseViewWithWindowManager"/> class.
        /// </summary>
        protected BaseViewWithWindowManager(IEnumerable<ViewRelation> relations,
                                            IServiceProvider serviceProvider,
                                            IDispatcherProxy dispatcherProxy) 
            : base(relations, serviceProvider, dispatcherProxy)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected sealed override object? Display(Func<object?> viewModelBuilder,
                                                  ViewRelation relation,
                                                  bool dialog,
                                                  NavigationArguments? arguments,
                                                  string? specializedId)
        {
            if (relation.View.IsAssignableTo(s_windowType))
            {
                var inst = this.ServiceProvider.GetRequiredService(relation.View) as Window;

                ArgumentNullException.ThrowIfNull(inst);

                var windowVM = viewModelBuilder();
                inst.DataContext = windowVM;

                // View model lifetime is managed by the view manager (persistence rules)
                if (inst is WindowExt windowExt)
                    windowExt.DisposeDataContextOnClosed = false;

                inst.Closed -= Window_Closed;
                inst.Closed += Window_Closed;

                if (dialog)
                    inst.ShowDialog();
                else
                    inst.Show();

                return windowVM;
            }

            return OnDisplay(viewModelBuilder, relation, dialog, arguments, specializedId);
        }

        /// <summary>
        /// Release the window view model following the persistence rules
        /// </summary>
        private void Window_Closed(object? sender, EventArgs e)
        {
            if (sender is Window window)
            {
                window.Closed -= Window_Closed;
                base.ViewClosed(window.DataContext);
            }
        }

        /// <inheritdoc cref="Display(Func{object?}, ViewRelation, bool, NavigationArguments?, string?)" />
        protected abstract object? OnDisplay(Func<object?> viewModelBuilder, ViewRelation relation, bool dialog, NavigationArguments? arguments, string? specializedId);

        #endregion
    }
}
