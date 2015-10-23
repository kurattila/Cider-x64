using System;
using System.Windows;

namespace Cider_x64.Helpers
{
    // Changed from http://tdanemar.wordpress.com/2009/11/15/using-the-visualstatemanager-with-the-model-view-viewmodel-pattern-in-wpf-or-silverlight/
    internal class VisualStateManager_Accessor : DependencyObject
    {
        public static string GetVisualStateName(DependencyObject obj)
        {
            return (string)obj.GetValue(VisualStateNameProperty);
        }

        public static void SetVisualStateName(DependencyObject obj, string value)
        {
            obj.SetValue(VisualStateNameProperty, value);
        }

        public static readonly string NoTransitionPostfix = "[NOTRANSITION]";

        public static readonly DependencyProperty VisualStateNameProperty =
            DependencyProperty.RegisterAttached(
            "VisualStateName",
            typeof(string),
            typeof(VisualStateManager_Accessor),
            new PropertyMetadata((s, e) =>
            {
                bool useTransitions = true;
                var propertyName = (string)e.NewValue;
                if (propertyName.Contains(NoTransitionPostfix))
                {
                    propertyName = propertyName.Replace(NoTransitionPostfix, "");
                    useTransitions = false;
                }

                var fe = s as FrameworkElement;
                if (fe == null)
                    throw new InvalidOperationException("FrameworkElement is needed");

                // Use 'GoToState()' for setting VSM states inside a ControlTemplate
                // System.Windows.VisualStateManager.GoToState(fe, (string)e.NewValue, true);
                System.Windows.VisualStateManager.GoToElementState(fe, propertyName, useTransitions);
            }));
    }
}
