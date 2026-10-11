// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.Builders
{
    using MARI.Toolbox.WPF.Abstractions.Views;

    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// Builder to associate view to view model
    /// </summary>
    internal sealed class ServiceViewBuilder : IViewBuilder
    {
        #region Fields

        private readonly IServiceCollection _serviceCollection;

        #endregion

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceViewBuilder"/> class.
        /// </summary>
        public ServiceViewBuilder(IServiceCollection serviceCollection)
        {
            this._serviceCollection = serviceCollection;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public IViewBuilder Register<TView, TViewModel>()
        {
            return Register<TView, TViewModel>(typeof(TView).Name);
        }

        /// <inheritdoc />
        public IViewBuilder Register<TView, TViewModel>(string key)
        {
            return Register<TView, TViewModel>(new ViewRelationInfo(key: key,
                                                                    viewPosition: ViewPositionEnum.Center,
                                                                    onlyOneAtTime: true,
                                                                    persistant: false));
        }

        /// <inheritdoc />
        public IViewBuilder Register<TView, TViewModel>(ViewRelationInfo viewRelationInfo)
        {
            this._serviceCollection.TryAddTransient(typeof(TView));
            this._serviceCollection.TryAddTransient(typeof(TViewModel));

            this._serviceCollection.AddSingleton(new ViewRelation(Guid.NewGuid(), typeof(TView), typeof(TViewModel), viewRelationInfo));
            return this;
        }

        #endregion
    }
}
