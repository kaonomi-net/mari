// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.Services
{
    using MARI.Toolbox.Abstractions.Proxies;
    using MARI.Toolbox.WPF.Abstractions;
    using MARI.Toolbox.WPF.Abstractions.Navigations;
    using MARI.Toolbox.WPF.Abstractions.Views;

    using Microsoft.Extensions.DependencyInjection;

    using System.Collections.Frozen;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Base view manager
    /// </summary>
    /// <seealso cref="IViewManager" />
    public abstract class BaseViewManager : IViewManager
    {
        #region Fields

        private readonly IReadOnlyDictionary<string, IReadOnlyCollection<ViewRelation>> _keyRelations;
        private readonly IReadOnlyCollection<ViewRelation> _relations;

        private readonly Dictionary<Type, object?> _persistantViewModel;

        private readonly IDispatcherProxy _dispatcherProxy;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseViewManager"/> class.
        /// </summary>
        protected BaseViewManager(IEnumerable<ViewRelation> relations,
                                  IServiceProvider serviceProvider,
                                  IDispatcherProxy dispatcherProxy)
        {
            this._relations = relations?.ToArray() ?? EnumerableHelper<ViewRelation>.ReadOnlyArray;

            this._keyRelations = this._relations.GroupBy(r => r.Info.Key ?? string.Empty)
                                                .ToFrozenDictionary(k => k.Key, grp => (IReadOnlyCollection<ViewRelation>)grp.ToArray(), StringComparer.OrdinalIgnoreCase);

            this._persistantViewModel = new Dictionary<Type, object?>();
            this._dispatcherProxy = dispatcherProxy;

            this.ServiceProvider = serviceProvider;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the service provider.
        /// </summary>
        public IServiceProvider ServiceProvider { get; }

        #endregion

        #region Method

        /// <inheritdoc />
        public void NavigateTo(string key, NavigationArguments? arguments = null, string? specializedId = null)
        {
            if (this._keyRelations.TryGetValue(key, out var relations))
            {
                if (relations.Count == 1)
                {
                    NavigateToImpl(relations.First(), arguments, specializedId: specializedId);
                    return;
                }
            }

            throw new KeyNotFoundException("Couldn't found a view associate to key '" + key + "'");
        }

        /// <inheritdoc />
        public void NavigateTo<TViewViewModel>(NavigationArguments? arguments = null, string? specializedId = null)
        {
            var vmTrait = typeof(TViewViewModel);

            var relations = this._relations.Where(r => r.ViewModel.IsAssignableTo(vmTrait) || r.ViewModel.IsAssignableFrom(vmTrait))
                                           .Select(r => (direct: r.ViewModel == vmTrait, r))
                                           .ToArray();

            // Search direct
            var result = relations.FirstOrDefault(r => r.direct).r;

            if (result is null && relations.Length == 1)
                result = relations.First().r;

            if (result is null)
                throw new KeyNotFoundException("Couldn't found a view associate to view model '" + vmTrait + "' or multiple items are associates: " + string.Join(", ", relations.Select(r => r.r)));

            NavigateToImpl(result, arguments, specializedId: specializedId);
        }

        /// <inheritdoc />
        public void Show<TView>(NavigationArguments? arguments = null, bool dialog = false, string? specializedId = null)
        {
            var viewTraits = typeof(TView);
            var viewRelation = this._relations.FirstOrDefault(r => r.View == viewTraits);

#pragma warning disable IDE0270 // Use coalesce expression
            if (viewRelation is null)
                throw new KeyNotFoundException("Couldn't found a view associate to view '" + viewTraits + "'");
#pragma warning restore IDE0270 // Use coalesce expression

            NavigateToImpl(viewRelation, arguments, dialog, specializedId);
        }

        #region Tools

        /// <summary>
        /// Navigates to view
        /// </summary>
        private void NavigateToImpl(ViewRelation viewRelation, NavigationArguments? arguments, bool dialog = false, string? specializedId = null)
        {
            this._dispatcherProxy.Send(() =>
            {
                var viewModel = Display(() => BuildViewModel(viewRelation), viewRelation, dialog, arguments, specializedId);
                ManagedNavigationNotification(viewModel, arguments);
            });
        }

        /// <summary>
        /// Builds the view model.
        /// </summary>
        private object? BuildViewModel(ViewRelation viewRelation)
        {
            var persistant = viewRelation.Info.Persistant == true;

            bool created = true;
            object? viewModel = null;

            if (persistant && this._persistantViewModel.TryGetValue(viewRelation.ViewModel, out var persistantViewModel))
            {
                viewModel = persistantViewModel;
                created = false;
            }

            if (created && viewModel is null)
                viewModel = this.ServiceProvider.GetRequiredService(viewRelation.ViewModel);

            if (persistant)
                this._persistantViewModel[viewRelation.ViewModel] = viewModel;

            return viewModel;
        }

        /// <summary>
        /// Manageds the navigation notification.
        /// </summary>
        private void ManagedNavigationNotification(object? viewModel, NavigationArguments? arguments)
        {
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    if (viewModel is ISupportNavigationTo navTo)
                        await navTo.OnNavigateToAsync(arguments);
                }
                catch (Exception ex)
                {
                    this._dispatcherProxy.Throw(ex);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Displays informations
        /// </summary>
        /// <remarks>
        ///     Called in the ui dispatcher
        /// </remarks>
        protected abstract object? Display(Func<object?> viewModelBuilder,
                                           ViewRelation relation,
                                           bool dialog,
                                           NavigationArguments? arguments,
                                           string? specializedId);

        /// <summary>
        /// Views the closed.
        /// </summary>
        /// <returns>
        ///     <c>true</c> the data context have been clean ; otherwise <c>false</c>
        /// </returns>
        protected virtual bool ViewClosed(object? dataContext)
        {
            var persistant = this._persistantViewModel.Any(p => object.ReferenceEquals(p.Value, dataContext));

            if (!persistant)
            {
                if (dataContext is IDisposable disposable)
                    disposable.Dispose();
                else if (dataContext is IAsyncDisposable asyncDisposable)
                    asyncDisposable.DisposeAsync().AsTask().ConfigureAwait(false);

                return true;
            }

            return false;
        }

        #endregion

        #endregion
    }
}
