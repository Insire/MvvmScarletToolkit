using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MvvmScarletToolkit
{
    public static class DependencyObjectExtensions
    {
        public static IEnumerable<DependencyObject> GetVisualDescendants(this DependencyObject visual)
        {
            if (visual is null)
            {
                yield break;
            }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                var child = VisualTreeHelper.GetChild(visual, i);
                yield return child;

                foreach (var subChild in GetVisualDescendants(child))
                {
                    yield return subChild;
                }
            }
        }

        public static T FindParent<T>(this DependencyObject dependencyObject)
            where T : DependencyObject
        {
            while (dependencyObject != null)
            {
                if (dependencyObject is T result)
                {
                    return result;
                }
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }

            return default;
        }

        public static T FindVisualChildDepthFirst<T>(this DependencyObject dependencyObject)
            where T : DependencyObject
        {
            return dependencyObject.FindVisualChildrenDepthFirst<T>(p => p is T).FirstOrDefault();
        }

        public static T FindVisualChildBreadthFirst<T>(this DependencyObject dependencyObject)
            where T : DependencyObject
        {
            return dependencyObject.FindVisualChildrenBreadthFirst<T>(p => p is T).FirstOrDefault();
        }

        public static IEnumerable<T> FindVisualChildrenBreadthFirst<T>(this DependencyObject dependencyObject)
            where T : DependencyObject
        {
            return dependencyObject.FindVisualChildrenBreadthFirst<T>(null);
        }

        /// <summary>
        /// Gets all children of the specified visual in the visual tree recursively.
        /// </summary>
        /// <param name="visual">The visual to get the visual children for.</param>
        /// <returns>All children of the specified visual in the visual tree recursively.</returns>
        public static IEnumerable<T> FindVisualChildrenBreadthFirst<T>(this DependencyObject dependencyObject, Func<T, bool> filter)
            where T : DependencyObject
        {
            if (dependencyObject is null)
            {
                yield break;
            }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
            {
                var child = VisualTreeHelper.GetChild(dependencyObject, i);
                if (child is T result && filter?.Invoke(result) != false)
                {
                    yield return result;
                }

                var grandchildren = child.FindVisualChildrenBreadthFirst(filter);
                foreach (var grandchild in grandchildren)
                {
                    yield return grandchild;
                }
            }
        }

        /// <summary>
        /// Gets all children of the specified visual in the visual tree recursively.
        /// </summary>
        /// <param name="visual">The visual to get the visual children for.</param>
        /// <returns>All children of the specified visual in the visual tree recursively.</returns>
        public static IEnumerable<T> FindVisualChildrenDepthFirst<T>(this DependencyObject dependencyObject, Func<T, bool> filter)
            where T : DependencyObject
        {
            if (!(dependencyObject is null))
            {
                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
                {
                    var child = VisualTreeHelper.GetChild(dependencyObject, i);
                    var grandchildren = child.FindVisualChildrenDepthFirst<T>(filter);

                    if (child is T result && filter?.Invoke(result) != false)
                    {
                        yield return result;
                    }

                    foreach (var grandchild in grandchildren)
                    {
                        yield return grandchild;
                    }
                }
            }
        }

        /// <summary>
        /// emulates wpf default behavior for looking up datatemplates based on a given type
        /// </summary>
        public static DataTemplate FindDataTemplateFor(this DependencyObject container, Type type)
        {
            return (container as FrameworkElement)?.FindDataTemplateFor(type);
        }

        /// <summary>
        /// emulates wpf default behavior for looking up datatemplates based on a given type
        /// </summary>
        public static DataTemplate FindDataTemplateFor(this FrameworkElement container, Type type)
        {
            return container?.FindResource(new DataTemplateKey(type)) as DataTemplate;
        }
    }
}
