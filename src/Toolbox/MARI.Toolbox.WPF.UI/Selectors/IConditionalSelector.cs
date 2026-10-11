// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Selectors
{
    using System.Windows;

    /// <summary>
    /// Conditional item selector
    /// </summary>
    public interface IConditionalSelector<TSelectItem>
    {
        /// <summary>
        /// Tries to get template base on input condition.
        /// </summary>
        bool TryGetItem(DependencyObject dependencyObject, object? dataContext, out TSelectItem? item);
    }

    /// <summary>
    /// Conditional selector dedicate to data template
    /// </summary>
    public interface IConditionalDataTemplate : IConditionalSelector<DataTemplate>
    {

    }

    /// <summary>
    /// Conditional selector dedicate to Style
    /// </summary>
    public interface IConditionalStyle : IConditionalSelector<Style>
    {

    }
}
