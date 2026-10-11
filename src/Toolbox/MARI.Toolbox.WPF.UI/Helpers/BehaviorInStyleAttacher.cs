// Copyright (c) Kaonomi.
// The MARI licenses this file to you under the MIT license.
// Produce by kaonomi & community (cf. docs/Teams.md)

namespace MARI.Toolbox.WPF.UI.Helpers
{
    using Microsoft.Xaml.Behaviors;

    using System.Collections;
    using System.Windows;

    // https://stackoverflow.com/a/58982495/24959500
    public static class BehaviorInStyleAttacher
    {
        #region Attached Properties

        public static readonly DependencyProperty BehaviorsProperty = DependencyProperty.RegisterAttached("Behaviors",
                                                                                                          typeof(IEnumerable),
                                                                                                          typeof(BehaviorInStyleAttacher),
                                                                                                          new UIPropertyMetadata(null, OnBehaviorsChanged));

        #endregion

        #region Getter and Setter of Attached Properties

        /// <summary>
        /// Gets or sets the behaviors.
        /// </summary>
        public static IEnumerable GetBehaviors(DependencyObject dependencyObject)
        {
            return (IEnumerable)dependencyObject.GetValue(BehaviorsProperty);
        }
        public static void SetBehaviors(
            DependencyObject dependencyObject, IEnumerable value)
        {
            dependencyObject.SetValue(BehaviorsProperty, value);
        }

        #endregion

        #region on property changed methods

        /// <summary>
        /// Called when [behaviors changed].
        /// </summary>
        private static void OnBehaviorsChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            var newBehaviorCollection = e.NewValue as IEnumerable;

            if (newBehaviorCollection is null)
                return;

            var behaviorCollection = Interaction.GetBehaviors(depObj);
            behaviorCollection.Clear();
            foreach (Behavior behavior in newBehaviorCollection)
            {
                // you need to make a copy of behavior in order to attach it to several controls
                var copy = behavior.Clone() as Behavior;
                behaviorCollection.Add(copy);
            }
        }

        #endregion
    }
}
