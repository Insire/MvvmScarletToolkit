using Xamarin.Forms;

namespace MvvmScarletToolkit.Xamarin.Forms
{
    public static class AnimatedProgress
    {
        public static BindableProperty CurrentValueProperty = BindableProperty.CreateAttached(
            "CurrentValue",
            typeof(double),
            typeof(AnimatedProgress),
            0.0d,
            BindingMode.OneWay,
            propertyChanged: OnCurrentValueChanged);

        public static double GetCurrentValue(BindableObject target)
        {
            return (double)target.GetValue(CurrentValueProperty);
        }

        public static void SetCurrentValue(BindableObject target, double value)
        {
            target.SetValue(CurrentValueProperty, value);
        }

        public static BindableProperty MaximumProperty = BindableProperty.CreateAttached(
            "Maximum",
            typeof(double),
            typeof(AnimatedProgress),
            100d,
            BindingMode.OneWay);

        public static double GetMaximum(BindableObject target)
        {
            return (double)target.GetValue(MaximumProperty);
        }

        public static void SetMaximum(BindableObject target, double value)
        {
            target.SetValue(MaximumProperty, value);
        }

        private static void OnCurrentValueChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is ProgressBar progressBar) || !(newValue is double d))
            {
                return;
            }

            progressBar.CancelAnimations();

            var maximum = GetMaximum(progressBar);
            var newProgress = d / maximum;

            if (double.IsNaN(newProgress))
            {
                progressBar.Progress = 0;
                return;
            }

            if (double.IsInfinity(newProgress))
            {
                progressBar.Progress = 0;
                return;
            }

            if (double.IsNegativeInfinity(newProgress))
            {
                progressBar.Progress = 0;
                return;
            }

            if (double.IsPositiveInfinity(newProgress))
            {
                progressBar.Progress = 0;
                return;
            }

            if (newProgress <= 0 || newProgress < progressBar.Progress)
            {
                progressBar.Progress = newProgress;
            }
            else
            {
                progressBar.ProgressTo(newProgress, 800, Easing.SinOut);
            }
        }
    }
}
