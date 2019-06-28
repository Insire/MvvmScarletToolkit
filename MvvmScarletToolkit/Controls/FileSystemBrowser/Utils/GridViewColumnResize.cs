using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    // https://stackoverflow.com/questions/560581/how-to-autosize-and-right-align-gridviewcolumn-data-in-wpf
    // source: http://lazycowprojects.tumblr.com/post/7063214400/wpf-c-listview-column-width-auto
    // https://github.com/rolfwessels/lazycowprojects/blob/master/Wpf/GridViewColumnResize.cs
    public static class GridViewColumnResize
    {
        public static readonly DependencyProperty WidthProperty = DependencyProperty.RegisterAttached(
            "Width",
            typeof(string),
            typeof(GridViewColumnResize),
            new PropertyMetadata(OnWidthChanged));

        public static readonly DependencyProperty GridViewColumnResizeBehaviorProperty = DependencyProperty.RegisterAttached(
            "GridViewColumnResizeBehavior",
            typeof(GridViewColumnResizeBehavior),
            typeof(GridViewColumnResize),
            new PropertyMetadata(default(GridViewColumnResizeBehavior)));

        public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached(
            "Enabled",
            typeof(bool),
            typeof(GridViewColumnResize),
            new PropertyMetadata(OnEnabledChanged));

        public static readonly DependencyProperty ListViewResizeBehaviorProperty = DependencyProperty.RegisterAttached(
            "ListViewResizeBehavior",
            typeof(ListViewResizeBehavior),
            typeof(GridViewColumnResize),
            new PropertyMetadata(default(ListViewResizeBehavior)));

        /// <summary>Helper for getting <see cref="WidthProperty"/> from <paramref name="obj"/>.</summary>
        /// <param name="obj"><see cref="DependencyObject"/> to read <see cref="WidthProperty"/> from.</param>
        /// <returns>Width property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static string GetWidth(DependencyObject obj)
        {
            return (string)obj.GetValue(WidthProperty);
        }

        /// <summary>Helper for setting <see cref="WidthProperty"/> on <paramref name="obj"/>.</summary>
        /// <param name="obj"><see cref="DependencyObject"/> to set <see cref="WidthProperty"/> on.</param>
        /// <param name="value">Width property value.</param>
        public static void SetWidth(DependencyObject obj, string value)
        {
            obj.SetValue(WidthProperty, value);
        }

        /// <summary>Helper for getting <see cref="EnabledProperty"/> from <paramref name="obj"/>.</summary>
        /// <param name="obj"><see cref="DependencyObject"/> to read <see cref="EnabledProperty"/> from.</param>
        /// <returns>Enabled property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static bool GetEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnabledProperty);
        }

        /// <summary>Helper for setting <see cref="EnabledProperty"/> on <paramref name="obj"/>.</summary>
        /// <param name="obj"><see cref="DependencyObject"/> to set <see cref="EnabledProperty"/> on.</param>
        /// <param name="value">Enabled property value.</param>
        public static void SetEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(EnabledProperty, value);
        }

        private static void OnWidthChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var element = dependencyObject as GridViewColumn;
            if (element is null)
            {
                Console.Error.WriteLine("Error: Expected type GridViewColumn but found " + dependencyObject.GetType().Name);
            }
            else
            {
                var behavior = GetGridViewColumnResizeBehavior(element);
                behavior.Width = e.NewValue as string;
            }
        }

        private static void OnEnabledChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var element = dependencyObject as ListView;
            if (element is null)
            {
                Console.Error.WriteLine("Error: Expected type ListView but found " + dependencyObject.GetType().Name);
            }
            else
            {
                var behavior = GetListViewResizeBehavior(element);
                behavior.Enabled = (bool)e.NewValue;
            }
        }

        [AttachedPropertyBrowsableForType(typeof(ListView))]
#pragma warning disable WPF0013 // CLR accessor for attached property must match registered type.
        private static ListViewResizeBehavior GetListViewResizeBehavior(ListView element)
#pragma warning restore WPF0013 // CLR accessor for attached property must match registered type.
        {
            if (!(element.GetValue(GridViewColumnResizeBehaviorProperty) is ListViewResizeBehavior behavior))
            {
                behavior = new ListViewResizeBehavior(element);
                element.SetCurrentValue(ListViewResizeBehaviorProperty, behavior);
            }

            return behavior;
        }

        [AttachedPropertyBrowsableForType(typeof(GridViewColumn))]
        private static GridViewColumnResizeBehavior GetGridViewColumnResizeBehavior(GridViewColumn element)
        {
            if (!(element.GetValue(GridViewColumnResizeBehaviorProperty) is GridViewColumnResizeBehavior behavior))
            {
                behavior = new GridViewColumnResizeBehavior(element);
                element.SetCurrentValue(GridViewColumnResizeBehaviorProperty, behavior);
            }

            return behavior;
        }

        /// <summary>
        /// ListViewResizeBehavior class that gets attached to the ListView control
        /// </summary>
        public class ListViewResizeBehavior
        {
            private const int Margin = 25;
            private const long RefreshTime = Timeout.Infinite;
            private const long Delay = 500;

            private readonly ListView _element;
            private readonly Timer _timer;

            public bool Enabled { get; set; }

            public ListViewResizeBehavior(ListView listView)
            {
                _element = listView ?? throw new ArgumentNullException(nameof(listView));

                listView.Loaded += OnLoaded;

                // Action for resizing and re-enable the size lookup
                // This stops the columns from constantly resizing to improve performance
                Action resizeAndEnableSize = () =>
                {
                    Resize();
                    _element.SizeChanged += OnSizeChanged;
                };
                _timer = new Timer(_ => Application.Current.Dispatcher.BeginInvoke(resizeAndEnableSize), null, Delay, RefreshTime);
            }

            private void OnLoaded(object sender, RoutedEventArgs e)
            {
                _element.SizeChanged += OnSizeChanged;
            }

            private void OnSizeChanged(object sender, SizeChangedEventArgs e)
            {
                if (e.WidthChanged)
                {
                    _element.SizeChanged -= OnSizeChanged;
                    _timer.Change(Delay, RefreshTime);
                }
            }

            private void Resize()
            {
                if (!Enabled)
                {
                    return;
                }

                var totalWidth = _element.ActualWidth;
                var gv = _element.View as GridView;
                if (gv != null)
                {
                    var allowedSpace = totalWidth - GetAllocatedSpace(gv);
                    allowedSpace -= Margin;
                    var totalPercentage = GridViewColumnResizeBehaviors(gv).Sum(x => x.Percentage);

                    foreach (var behavior in GridViewColumnResizeBehaviors(gv))
                    {
                        behavior.SetWidth(allowedSpace, totalPercentage);
                    }
                }
            }

            private static IEnumerable<GridViewColumnResizeBehavior> GridViewColumnResizeBehaviors(GridView gv)
            {
                foreach (var t in gv.Columns)
                {
                    var gridViewColumnResizeBehavior =
                        t.GetValue(GridViewColumnResizeBehaviorProperty) as GridViewColumnResizeBehavior;
                    if (gridViewColumnResizeBehavior != null)
                    {
                        yield return gridViewColumnResizeBehavior;
                    }
                }
            }

            private static double GetAllocatedSpace(GridView gv)
            {
                double totalWidth = 0;
                foreach (var t in gv.Columns)
                {
                    var gridViewColumnResizeBehavior = t.GetValue(GridViewColumnResizeBehaviorProperty) as GridViewColumnResizeBehavior;
                    if (gridViewColumnResizeBehavior != null)
                    {
                        if (gridViewColumnResizeBehavior.IsStatic)
                        {
                            totalWidth += gridViewColumnResizeBehavior.StaticWidth;
                        }
                    }
                    else
                    {
                        totalWidth += t.ActualWidth;
                    }
                }

                return totalWidth;
            }
        }

        /// <summary>
        /// GridViewColumn class that gets attached to the GridViewColumn control
        /// </summary>
        public class GridViewColumnResizeBehavior
        {
            private readonly GridViewColumn _element;

            public bool IsStatic => StaticWidth >= 0;
            public double StaticWidth => double.TryParse(Width, out var result) ? result : -1;
            public double Percentage => !IsStatic ? Mulitplier * 100 : 0;

            public double Mulitplier
            {
                get
                {
                    if (Width == "*" || Width == "1*")
                    {
                        return 1;
                    }

                    if (Width.EndsWith("*") && double.TryParse(Width.Substring(0, Width.Length - 1), out var perc))
                    {
                        return perc;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }

            public string Width { get; set; }

            public GridViewColumnResizeBehavior(GridViewColumn element)
            {
                _element = element;
            }

            public void SetWidth(double allowedSpace, double totalPercentage)
            {
                _element.SetCurrentValue(GridViewColumn.WidthProperty, IsStatic
                    ? StaticWidth
                    : allowedSpace * (Percentage / totalPercentage));
            }
        }
    }
}
