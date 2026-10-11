// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Selectors
{
    using System.Windows;

    /// <summary>
    /// 
    /// </summary>
    public abstract class TypeBaseConditonalSelector<TItem> : ConditonalBaseItem<TItem>, IConditionalSelector<TItem>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the match type.
        /// </summary>
        public Type? Type { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override bool ValidateItemUsage(DependencyObject dependencyObject, object? dataContext)
        {
            if (this.Type is null)
                return false;

            return (dataContext is not null && dataContext.GetType().IsAssignableTo(this.Type)) ||
                   (dependencyObject is not null && dependencyObject.GetType().IsAssignableTo(this.Type));
        }

        #endregion
    }

    public sealed class TypeConditonalTemplateSelector : TypeBaseConditonalSelector<DataTemplate>, IConditionalDataTemplate
    {
    }

    public sealed class TypeConditonalStyleSelector : TypeBaseConditonalSelector<Style>, IConditionalStyle
    {
    }
}
