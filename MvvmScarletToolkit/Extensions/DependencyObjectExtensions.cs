using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MvvmScarletToolkit
{
    public static class DependencyObjectExtensions
    {
        /// <summary>
        ///     Gets the first child of the specified visual that is of tyoe <typeparamref name="T" />
        ///     in the visual tree recursively.
        /// </summary>
        /// <param name="visual">The visual to get the visual children for.</param>
        /// <returns>
        ///     The first child of the specified visual that is of tyoe <typeparamref name="T" /> of the
        ///     specified visual in the visual tree recursively or <c>null</c> if none was found.
        /// </returns>
        public static T GetVisualDescendant<T>(this DependencyObject visual)
            where T : DependencyObject
        {
            return (T)visual.GetVisualDescendants().FirstOrDefault(d => d is T);
        }

        /// <summary>
        ///     Gets all children of the specified visual in the visual tree recursively.
        /// </summary>
        /// <param name="visual">The visual to get the visual children for.</param>
        /// <returns>All children of the specified visual in the visual tree recursively.</returns>
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
