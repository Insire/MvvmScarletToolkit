using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace MvvmScarletToolkit
{
    public static class DependencyObjectExtensions
    {
        public static IEnumerable<T> FindVisualChildrenBreadthFirst<T>(this DependencyObject dependencyObject)
            where T : DependencyObject
        {
            return dependencyObject.FindVisualChildrenBreadthFirst<T>(null);
        }

        public static IEnumerable<T> FindVisualChildrenBreadthFirst<T>(this DependencyObject dependencyObject, Func<T, bool> filter)
            where T : DependencyObject
        {
            if (!(dependencyObject is null))
            {
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
        }

        public static IEnumerable<T> FindVisualChildrenDepthFirst<T>(this DependencyObject dependencyObject)
            where T : DependencyObject
        {
            return dependencyObject.FindVisualChildrenDepthFirst<T>(null);
        }

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
    }
}
