// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Selectors
{
    using System.Windows;

    /// <summary>
    /// 
    /// </summary>
    public abstract class ConditonalBaseItem<TItem> : DependencyObject, IConditionalSelector<TItem>
    {
        #region Fields

        public static readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item),
                                                                                             typeof(TItem),
                                                                                             typeof(ConditonalBaseItem<TItem>),
                                                                                             new PropertyMetadata(default(TItem)));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the template.
        /// </summary>
        public TItem Item
        {
            get { return (TItem)GetValue(ItemProperty); }
            set { SetValue(ItemProperty, value); }
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public bool TryGetItem(DependencyObject dependencyObject, object? dataContext, out TItem? conditionalItem)
        {
            conditionalItem = this.Item;

            return ValidateItemUsage(dependencyObject, dataContext);
        }

        /// <summary>
        /// Validates item usage condition
        /// </summary>
        protected abstract bool ValidateItemUsage(DependencyObject dependencyObject, object? dataContext);

        #endregion
    }
}
