using System;
using Xamarin.Forms;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// base class providing the currently associated control that also forwwards updates to the bindingcontext as it changes on the control
    /// </summary>
    /// <typeparam name="T">a control</typeparam>
    public abstract class BehaviorBase<T> : Behavior<T>
        where T : BindableObject
    {
        protected T? AssociatedObject { get; private set; }

        protected override void OnAttachedTo(T bindable)
        {
            base.OnAttachedTo(bindable);
            AssociatedObject = bindable;

            if (bindable.BindingContext != null)
            {
                BindingContext = bindable.BindingContext;
            }

            bindable.BindingContextChanged += OnBindingContextChanged;
        }

        protected override void OnDetachingFrom(T bindable)
        {
            base.OnDetachingFrom(bindable);

            bindable.BindingContextChanged -= OnBindingContextChanged;
            AssociatedObject = null;
        }

        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            OnBindingContextChanged();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            BindingContext = AssociatedObject?.BindingContext;
        }
    }
}
