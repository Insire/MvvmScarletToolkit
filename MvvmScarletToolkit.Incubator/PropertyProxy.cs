using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace MvvmScarletToolkit
{
    public class PopupPropertyProxy : Behavior<Popup>
    {
        public delegate void EventRaised(object sender, EventArgs e);

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Closed += AssociatedObject_Closed;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Closed -= AssociatedObject_Closed;
            base.OnDetaching();
        }

        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>Identifies the <see cref="Source"/> dependency property.</summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source),
            typeof(string),
            typeof(PopupPropertyProxy),
            new PropertyMetadata(default(string)));

        public string Target
        {
            get { return (string)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        /// <summary>Identifies the <see cref="Target"/> dependency property.</summary>
        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(nameof(Target),
            typeof(string),
            typeof(PopupPropertyProxy),
            new PropertyMetadata(default(string)));

        private void AssociatedObject_Closed(object sender, EventArgs e)
        {
            SetCurrentValue(TargetProperty, Source);
        }

        // source property from anywhere as dpprop with oneway binding
        // target property from some control as dpprop
        // as behavior, so that we can attach to arbitrary events
    }
}
