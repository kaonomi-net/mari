// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.Builders
{
    using MARI.Toolbox.WPF.Abstractions.Views;

    /// <summary>
    /// 
    /// </summary>
    public interface IViewBuilder
    {
        /// <summary>
        /// Associate view and view model
        /// </summary>
        IViewBuilder Register<TView, TViewModel>();

        /// <summary>
        /// Associate view and view model with a specific <paramref name="key"/>
        /// </summary>
        IViewBuilder Register<TView, TViewModel>(string key);

        /// <summary>
        /// Associate view and view model with a specific <paramref name="viewRelationInfo"/>
        /// </summary>
        IViewBuilder Register<TView, TViewModel>(ViewRelationInfo viewRelationInfo);
    }
}
