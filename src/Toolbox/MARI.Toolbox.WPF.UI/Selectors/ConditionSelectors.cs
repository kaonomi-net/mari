
namespace MARI.Toolbox.WPF.UI.Selectors
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;

    /// <summary>
    /// Selector about DataTemplate
    /// </summary>
    [ContentProperty(nameof(Conditions))]
    public sealed class ConditionalDataTemplateSelector : DataTemplateSelector
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalDataTemplateSelector"/> class.
        /// </summary>
        public ConditionalDataTemplateSelector()
        {
            this.Conditions = new List<IConditionalDataTemplate>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the default DataTemplate.
        /// </summary>
        public DataTemplate? Default { get; set; }

        /// <summary>
        /// Gets or sets the conditions.
        /// </summary>
        public List<IConditionalDataTemplate> Conditions { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (this.Conditions is not null && this.Conditions.Any())
            {
                var dataCtx = item is FrameworkElement ife
                                        ? ife.DataContext
                                        : item ?? (container as FrameworkElement)?.DataContext;

                foreach (var cond in this.Conditions)
                {
                    if (cond.TryGetItem(container, dataCtx, out var tmpl) && tmpl is not null)
                        return tmpl;
                }
            }

            return this.Default;
        }

        #endregion
    }
    /// <summary>
    /// Selector about Style
    /// </summary>
    [ContentProperty(nameof(Conditions))]
    public sealed class ConditionalStyleSelector : StyleSelector
    {
        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionalStyleSelector"/> class.
        /// </summary>
        public ConditionalStyleSelector()
        {
            this.Conditions = new List<IConditionalStyle>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the default Style.
        /// </summary>
        public Style? Default { get; set; }

        /// <summary>
        /// Gets or sets the conditions.
        /// </summary>
        public List<IConditionalStyle> Conditions { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override Style? SelectStyle(object item, DependencyObject container)
        {
            if (this.Conditions is not null && this.Conditions.Any())
            {
                var dataCtx = item is FrameworkElement ife
                                        ? ife.DataContext
                                        : item ?? (container as FrameworkElement)?.DataContext;

                foreach (var cond in this.Conditions)
                {
                    if (cond.TryGetItem(container, dataCtx, out var tmpl) && tmpl is not null)
                        return tmpl;
                }
            }

            return this.Default;
        }

        #endregion
    }
}