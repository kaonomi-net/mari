// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Behaviors
{
    using Microsoft.Xaml.Behaviors;

    using System.Windows;
    using System.Windows.Controls;

    // cref = https://stackoverflow.com/questions/1000040/data-binding-to-selecteditem-in-a-wpf-treeview

    public sealed class BindableSelectedItemBehavior : Behavior<TreeView>
    {
        #region Fields

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem),
                                                                                                     typeof(object),
                                                                                                     typeof(BindableSelectedItemBehavior),
                                                                                                     new UIPropertyMetadata(null, OnSelectedItemChanged));

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Called when [selected item changed].
        /// </summary>
        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var item = e.NewValue as TreeViewItem;
            item?.SetValue(TreeViewItem.IsSelectedProperty, true);
        }

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            base.OnDetaching();

            if (this.AssociatedObject != null)
                this.AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
        }

        /// <summary>
        /// Called when [TreeView selected item changed].
        /// </summary>
        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.SelectedItem = e.NewValue;
        }

        #endregion
    }
}
