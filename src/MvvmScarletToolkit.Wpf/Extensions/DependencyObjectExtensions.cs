using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace MvvmScarletToolkit
{
    public static class DependencyObjectExtensions
    {
        public static IEnumerable<DependencyObject> GetVisualDescendants(this DependencyObject? visual)
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

        public static DependencyObject? FindTopMostParent(this DependencyObject? dependencyObject)
        {
            while (dependencyObject != null)
            {
                var isVisual = dependencyObject is Visual or Visual3D;
                dependencyObject = isVisual
                    ? VisualTreeHelper.GetParent(dependencyObject)
                    : LogicalTreeHelper.GetParent(dependencyObject);
            }

            return null;
        }

        public static T? FindParent<T>(this DependencyObject dependencyObject)
            where T : DependencyObject
        {
            var parent = dependencyObject;
            while (parent != null)
            {
                if (parent is T result)
                {
                    return result;
                }

                var isVisual = parent is Visual or Visual3D;
                parent = isVisual
                    ? VisualTreeHelper.GetParent(dependencyObject)
                    : LogicalTreeHelper.GetParent(dependencyObject);
            }

            return null;
        }

        public static T? FindVisualChildBreadthFirst<T>(this DependencyObject dependencyObject)
            where T : DependencyObject
        {
            return dependencyObject.FindVisualChildrenBreadthFirst<T>(null).FirstOrDefault();
        }

        public static IEnumerable<T> FindVisualChildrenBreadthFirst<T>(this DependencyObject dependencyObject)
            where T : DependencyObject
        {
            return dependencyObject.FindVisualChildrenBreadthFirst<T>(null);
        }

        /// <summary>
        /// Gets all children of the specified visual in the visual tree recursively.
        /// </summary>
        /// <param name="dependencyObject">The visual to get the visual children for.</param>
        /// <param name="filter"></param>
        /// <returns>All children of the specified visual in the visual tree recursively.</returns>
        public static IEnumerable<T> FindVisualChildrenBreadthFirst<T>(this DependencyObject? dependencyObject, Func<T, bool>? filter)
            where T : DependencyObject
        {
            if (dependencyObject is null)
            {
                yield break;
            }

            var count = VisualTreeHelper.GetChildrenCount(dependencyObject);
            var children = new DependencyObject[count];

            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(dependencyObject, i);
                children[i] = child;

                if (child is T result && filter?.Invoke(result) != false)
                {
                    yield return result;
                }
            }

            for (var i = 0; i < count; i++)
            {
                var child = children[i];
                foreach (var grandchild in child.FindVisualChildrenBreadthFirst(filter))
                {
                    yield return grandchild;
                }
            }
        }

        /// <summary>
        /// Gets all children of the specified visual in the visual tree recursively including itself.
        /// </summary>
        /// <param name="dependencyObject">The visual to get the visual children for.</param>
        /// <returns>All children of the specified visual in the visual tree recursively including itself.</returns>
        public static IEnumerable<T> FindVisualChildrenBreadthFirstOrSelf<T>(this DependencyObject dependencyObject, Func<T, bool>? filter = null)
            where T : DependencyObject
        {
            if (dependencyObject is T self && filter?.Invoke(self) != false)
            {
                yield return self;
            }

            foreach (var child in dependencyObject.FindVisualChildrenBreadthFirst<T>(filter))
            {
                yield return child;
            }
        }

        public static T? FindVisualChildDepthFirst<T>(this DependencyObject dependencyObject)
            where T : DependencyObject
        {
            return dependencyObject.FindVisualChildrenDepthFirst<T>(null).FirstOrDefault();
        }

        public static IEnumerable<T> FindVisualChildrenDepthFirst<T>(this DependencyObject dependencyObject)
            where T : DependencyObject
        {
            return dependencyObject.FindVisualChildrenDepthFirst<T>(null);
        }

        /// <summary>
        /// Gets all children of the specified visual in the visual tree recursively.
        /// </summary>
        /// <param name="dependencyObject">The visual to get the visual children for.</param>
        /// <returns>All children of the specified visual in the visual tree recursively.</returns>
        public static IEnumerable<T> FindVisualChildrenDepthFirst<T>(this DependencyObject? dependencyObject, Func<T, bool>? filter = null)
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

                foreach (var grandchild in child.FindVisualChildrenDepthFirst(filter))
                {
                    yield return grandchild;
                }
            }
        }

        /// <summary>
        /// Gets all children of the specified visual in the visual tree recursively including itself.
        /// </summary>
        /// <param name="dependencyObject">The visual to get the visual children for.</param>
        /// <returns>All children of the specified visual in the visual tree recursively including itself.</returns>
        public static IEnumerable<T> FindVisualChildrenDepthFirstOrSelf<T>(this DependencyObject dependencyObject, Func<T, bool>? filter = null)
            where T : DependencyObject
        {
            if (dependencyObject is T self && filter?.Invoke(self) != false)
            {
                yield return self;
            }

            foreach (var child in dependencyObject.FindVisualChildrenDepthFirst<T>(filter))
            {
                yield return child;
            }
        }

        /// <summary>
        /// emulates wpf default behavior for looking up datatemplates based on a given type
        /// </summary>
        public static DataTemplate? FindDataTemplateFor(this DependencyObject container, Type type)
        {
            return (container as FrameworkElement)?.FindDataTemplateFor(type);
        }

        /// <summary>
        /// emulates wpf default behavior for looking up datatemplates based on a given type
        /// </summary>
        public static DataTemplate? FindDataTemplateFor(this FrameworkElement container, Type type)
        {
            return container?.FindResource(new DataTemplateKey(type)) as DataTemplate;
        }
    }
}
