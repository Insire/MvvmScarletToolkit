using System.Threading.Tasks;
using Xamarin.Forms;

namespace MvvmScarletToolkit.Xamarin.Forms
{
    /// <summary>
    /// Behavior that flashes a control, when its being tapped on
    /// </summary>
    // usage:
    // <Control.Behaviors>
    //    <mvvm:ViewTappedAnimationBehavior />
    // </Control.Behaviors>
    public sealed class ViewTappedAnimationBehavior : BehaviorBase<View>
    {
        protected override void OnAttachedTo(View bindable)
        {
            base.OnAttachedTo(bindable);

            var gesture = new TapGestureRecognizer();

            gesture.Tapped += Gesture_Tapped;

            bindable.GestureRecognizers.Add(gesture);

            base.OnAttachedTo(bindable);
        }

        private async void Gesture_Tapped(object sender, System.EventArgs e)
        {
            if (AssociatedObject is null)
            {
                return;
            }

            if (!AssociatedObject.IsEnabled)
            {
                return;
            }

            await AnimateItem(AssociatedObject).ConfigureAwait(true);
        }

        protected override void OnDetachingFrom(View bindable)
        {
            if (AssociatedObject is null)
            {
                return;
            }

            foreach (var gesture in AssociatedObject.GestureRecognizers)
            {
                if (gesture is TapGestureRecognizer tapGesture)
                {
                    tapGesture.Tapped -= Gesture_Tapped;
                }
            }

            base.OnDetachingFrom(bindable);
        }

        private bool _isAnimating;

        private async Task AnimateItem(View uiElement)
        {
            const uint AnimationDuration = 150;

            if (_isAnimating)
            {
                return;
            }

            _isAnimating = true;

            var originalOpacity = uiElement.Opacity;

            await uiElement.FadeTo(.5, AnimationDuration / 2, Easing.CubicIn).ConfigureAwait(true);
            await uiElement.FadeTo(originalOpacity, AnimationDuration / 2, Easing.CubicIn).ConfigureAwait(true);

            _isAnimating = false;
        }
    }
}
