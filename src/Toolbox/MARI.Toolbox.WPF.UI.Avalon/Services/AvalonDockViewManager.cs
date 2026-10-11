namespace MARI.Toolbox.WPF.UI.Avalon.Services
{
    using AvalonDock.Layout;

    using MARI.Toolbox.Abstractions.Proxies;
    using MARI.Toolbox.WPF.Abstractions;
    using MARI.Toolbox.WPF.Abstractions.Navigations;
    using MARI.Toolbox.WPF.Abstractions.Views;
    using MARI.Toolbox.WPF.Services;
    using MARI.Toolbox.WPF.UI.Services;

    using Microsoft.Extensions.DependencyInjection;

    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// View manager to display view into <see cref="AvalonDock.DockingManager"/>
    /// </summary>
    /// <seealso cref="BaseViewManager" />
    public sealed class AvalonDockViewManager : BaseViewWithWindowManager
    {
        #region Fields

        private AvalonDock.DockingManager? _avalonDock;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="AvalonDockViewManager"/> class.
        /// </summary>
        public AvalonDockViewManager(IEnumerable<ViewRelation> relations,
                                     IServiceProvider serviceProvider,
                                     IDispatcherProxy dispatcherProxy)
            : base(relations, serviceProvider, dispatcherProxy)
        {
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override object? OnDisplay(Func<object?> viewModelBuilder,
                                             ViewRelation relation,
                                             bool dialog,
                                             NavigationArguments? arguments,
                                             string? specializedId)
        {
            ArgumentNullException.ThrowIfNull(this._avalonDock);

            var viewUid = relation.Uid.ToString();

            if (!string.IsNullOrEmpty(specializedId))
                viewUid += ":" + specializedId;

            //this._avalonDock.FloatingWindows
            var view = GetOpenedViews(this._avalonDock.Layout).FirstOrDefault(c => c is LayoutContent layoutContent && layoutContent.ContentId == viewUid) as LayoutContent;

            var viewModel = (view?.Content as FrameworkElement)?.DataContext ?? viewModelBuilder();

            if (view is null)
                view = CreateView(relation);

            if (view.Content is null)
                SetViewWithViewModel(relation, viewUid, view, viewModel);

            view.Closed -= View_Closed;
            view.Closed += View_Closed;

            if (view is LayoutAnchorable layoutAnchorable)
            {
                layoutAnchorable.Show();

                layoutAnchorable.IsVisibleChanged -= LayoutAnchorable_IsVisibleChanged;
                layoutAnchorable.IsVisibleChanged += LayoutAnchorable_IsVisibleChanged;
            }
            else if (view is LayoutDocument layoutDocument)
            {
                layoutDocument.IsActive = true;
            }

            return viewModel;
        }

        /// <summary>
        /// Sets the view with view model.
        /// </summary>
        private void SetViewWithViewModel(ViewRelation relation, string viewUid, LayoutContent view, object? viewModel)
        {
            var viewUI = this.ServiceProvider.GetRequiredService(relation.View) as FrameworkElement;
            if (viewUI is not null)
            {
                viewUI.DataContext = viewModel;
                view.Content = viewUI;
                view.ContentId = viewUid;
            }

            if (viewUI is not null && !double.IsNaN(viewUI.MinWidth) && viewUI.MinWidth > 0)
            {
                if (view.Parent is LayoutAnchorablePaneGroup grp)
                    grp.DockMinWidth = viewUI.MinWidth;
                else if (view.Parent is LayoutAnchorablePane anchorablePane)
                    anchorablePane.DockMinWidth = viewUI.MinWidth;
            }

            if (viewUI is not null && !double.IsNaN(viewUI.MinHeight) && viewUI.MinHeight > 0)
            {
                if (view.Parent is LayoutAnchorablePaneGroup grp)
                    grp.DockMinHeight = viewUI.MinHeight;
                else if (view.Parent is LayoutAnchorablePane anchorablePane)
                    anchorablePane.DockMinHeight = viewUI.MinHeight;
            }

            if (viewModel is ISupportViewTitle)
            {
                BindingOperations.SetBinding(view, LayoutContent.TitleProperty, new Binding()
                {
                    Path = new PropertyPath(nameof(ISupportViewTitle.Title)),
                    Source = viewModel
                });
            }
        }

        /// <summary>
        /// Creates the view.
        /// </summary>
        private LayoutContent CreateView(ViewRelation relation)
        {
            LayoutContent? view;
            if (relation.Info.ViewPosition is null || relation.Info.ViewPosition == ViewPositionEnum.Center)
            {
                var doc = new LayoutDocument();
                view = doc;

                var docPane = this._avalonDock!.Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();

                ArgumentNullException.ThrowIfNull(docPane);
                docPane?.Children.Add(doc);
            }
            else
            {
                var anchorable = new LayoutAnchorable();
                view = anchorable;

                switch (relation.Info.ViewPosition)
                {
                    case ViewPositionEnum.Top:
                        anchorable.AddToLayout(this._avalonDock, AnchorableShowStrategy.Top);
                        break;

                    case ViewPositionEnum.Left:
                        anchorable.AddToLayout(this._avalonDock, AnchorableShowStrategy.Left);
                        break;

                    case ViewPositionEnum.Bottom:
                        anchorable.AddToLayout(this._avalonDock, AnchorableShowStrategy.Bottom);
                        break;

                    case ViewPositionEnum.Right:
                        anchorable.AddToLayout(this._avalonDock, AnchorableShowStrategy.Right);
                        break;
                }
            }

            return view;
        }

        /// <summary>
        /// Layouts the anchorable is visible changed.
        /// </summary>
        private void LayoutAnchorable_IsVisibleChanged(object? sender, EventArgs e)
        {
            var layout = sender as LayoutAnchorable;
            var fe = layout?.Content as FrameworkElement;

            if (layout is null || fe is null)
                return;

            layout.IsVisibleChanged -= LayoutAnchorable_IsVisibleChanged;

            if (base.ViewClosed(fe.DataContext))
                layout.Content = null;
        }

        /// <summary>
        /// Views the closed.
        /// </summary>
        private void View_Closed(object? sender, EventArgs e)
        {
            if (sender is LayoutContent content)
                content.Closed -= View_Closed;

            base.ViewClosed(((sender as LayoutContent)?.Content as FrameworkElement)?.DataContext);
        }

        /// <summary>
        /// Registers the dock manager.
        /// </summary>
        public void RegisterDockManager(AvalonDock.DockingManager avalonDock)
        {
            this._avalonDock = avalonDock;
        }

        /// <summary>
        /// Get a value indicating if the view associate to specific key is openned
        /// </summary>
        private static IEnumerable<LayoutContent> GetOpenedViews(ILayoutContainer layoutContainer)
        {
            if (layoutContainer != null && layoutContainer.Children != null && layoutContainer.ChildrenCount > 0)
            {
                var viewFounded = layoutContainer.Children
                                                 .OfType<LayoutContent>();

                foreach (var view in viewFounded)
                    yield return view;
            }

            if (layoutContainer is not null && layoutContainer.Children != null)
            {
                foreach (var container in layoutContainer.Children.OfType<ILayoutContainer>())
                {
                    var views = GetOpenedViews(container);
                    foreach (var view in views)
                        yield return view;
                }
            }
        }

        #endregion
    }
}
