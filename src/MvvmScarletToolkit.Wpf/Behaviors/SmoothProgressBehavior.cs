using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace MvvmScarletToolkit.Wpf
{
    /// <summary>
    /// Behavior that smoothes out values changes of a <see cref="ProgressBar"/>
    /// </summary>
    // usage:
    // <i:Interaction.Behaviors>
    //    <mvvm:SmoothProgressBehavior />
    // </ i:Interaction.Behaviors>
    public sealed class SmoothProgressBehavior : Behavior<ProgressBar>
    {
        public double Percentage
        {
            get { return (double)GetValue(PercentageProperty); }
            set { SetValue(PercentageProperty, value); }
        }

        public static readonly DependencyProperty PercentageProperty = DependencyProperty.Register(
            nameof(Percentage),
            typeof(double),
            typeof(SmoothProgressBehavior),
            new PropertyMetadata(0d, OnPercentageChanged));

        private static void OnPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not SmoothProgressBehavior behavior)
            {
                return;
            }

            if (behavior.AssociatedObject is null)
            {
                return;
            }

            behavior.OnPercentageChanged((double)e.OldValue, (double)e.NewValue);
        }

        private void OnPercentageChanged(double oldValue, double newValue)
        {
            var animation = new DoubleAnimation(oldValue, newValue, new TimeSpan(0, 0, 0, 0, 250))
            {
                EasingFunction = new QuadraticEase()
                {
                    EasingMode = EasingMode.EaseInOut,
                }
            };

            AssociatedObject.BeginAnimation(System.Windows.Controls.Primitives.RangeBase.ValueProperty, animation, HandoffBehavior.SnapshotAndReplace);
        }
    }
}
