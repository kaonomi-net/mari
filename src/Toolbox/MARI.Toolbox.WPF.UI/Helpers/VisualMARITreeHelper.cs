// Keep : System.Windows
namespace System.Windows
{
    using System.Windows.Media;

    public static class VisualMARITreeHelper
    {
        /// <summary>
        /// Gets the tree root.
        /// </summary>
        /// <param name="dependencyObject">The dependency object.</param>
        /// <returns></returns>
        public static DependencyObject GetTreeRoot(this DependencyObject dependencyObject)
        {
            var parent = VisualTreeHelper.GetParent(dependencyObject);  
            if (parent != null)
                return GetTreeRoot(parent);

            return dependencyObject;
        }

        /// <summary>
        /// Gets the first parent that match the conditions.
        /// </summary>
        public static TParentType? GetParent<TParentType>(this DependencyObject dependencyObject, Func<TParentType, bool>? filter = null)
            where TParentType : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(dependencyObject);

            if (parent is TParentType tParent && (filter == null || filter(tParent)))
                return tParent;

            if (parent != null)
                return GetParent<TParentType>(parent, filter);

            return null;
        }
    }
}
