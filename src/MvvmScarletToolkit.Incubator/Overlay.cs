using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MvvmScarletToolkit
{
    [TemplatePart(Name = "PART_OverlayAdorner", Type = typeof(AdornerDecorator))]
    public class Overlay : ContentControl
    {
        public static readonly DependencyProperty OverlayContentProperty = DependencyProperty.Register(
                                                    nameof(OverlayContent),
                                                    typeof(UIElement),
                                                    typeof(Overlay),
                                                    new FrameworkPropertyMetadata(new PropertyChangedCallback(OnOverlayContentChanged)));

        public static readonly DependencyProperty IsOverlayContentVisibleProperty = DependencyProperty.Register(
                                                    nameof(IsOverlayContentVisible),
                                                    typeof(bool),
                                                    typeof(Overlay),
                                                    new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsOverlayContentVisibleChanged)));

        private UIElementAdorner _uiElementAdorner;

        static Overlay()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Overlay), new FrameworkPropertyMetadata(typeof(Overlay)));
        }

        [Category("Overlay")]
        public UIElement OverlayContent
        {
            get { return (UIElement)GetValue(OverlayContentProperty); }
            set { SetValue(OverlayContentProperty, value); }
        }

        [Category("Overlay")]
        public bool IsOverlayContentVisible
        {
            get { return (bool)GetValue(IsOverlayContentVisibleProperty); }
            set { SetValue(IsOverlayContentVisibleProperty, value); }
        }

        private static void OnOverlayContentChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (!(dp is Overlay overlay))
            {
                return;
            }

            if (!overlay.IsOverlayContentVisible)
            {
                return;
            }

            overlay.RemoveOverlayContent();
            overlay.AddOverlayContent();
        }

        private static void OnIsOverlayContentVisibleChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (!(dp is Overlay overlay))
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                overlay.AddOverlayContent();
            }
            else
            {
                overlay.RemoveOverlayContent();
            }
        }

        private void AddOverlayContent()
        {
            if (OverlayContent == null)
            {
                return;
            }

            _uiElementAdorner = new UIElementAdorner(this, OverlayContent);
            _uiElementAdorner.Add();

            var parentAdorner = AdornerLayer.GetAdornerLayer(this);
            parentAdorner.Add(_uiElementAdorner);
        }

        private void RemoveOverlayContent()
        {
            if (_uiElementAdorner == null)
            {
                return;
            }

            var parentAdorner = AdornerLayer.GetAdornerLayer(this);
            parentAdorner.Remove(_uiElementAdorner);

            _uiElementAdorner.Remove();
            _uiElementAdorner = null;
        }

        private class UIElementAdorner : Adorner
        {
            private List<UIElement> _logicalChildren;
            private readonly UIElement _uiElement;

            public UIElementAdorner(UIElement adornedElement, UIElement element)
                : base(adornedElement)
            {
                _uiElement = element;
            }

            public void Add()
            {
                AddLogicalChild(_uiElement);
                AddVisualChild(_uiElement);
            }

            public void Remove()
            {
                RemoveLogicalChild(_uiElement);
                RemoveVisualChild(_uiElement);
            }

            protected override Size MeasureOverride(Size constraint)
            {
                _uiElement.Measure(constraint);
                return _uiElement.DesiredSize;
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                var location = new Point(0, 0);
                var rect = new Rect(location, finalSize);

                _uiElement.Arrange(rect);

                return finalSize;
            }

            protected override int VisualChildrenCount => 1;

            protected override Visual GetVisualChild(int index)
            {
                if (index != 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return _uiElement;
            }

            protected override IEnumerator LogicalChildren
            {
                get
                {
                    if (_logicalChildren == null)
                    {
                        _logicalChildren = new List<UIElement>
                        {
                            _uiElement
                        };
                    }

                    return _logicalChildren.GetEnumerator();
                }
            }
        }
    }
}
