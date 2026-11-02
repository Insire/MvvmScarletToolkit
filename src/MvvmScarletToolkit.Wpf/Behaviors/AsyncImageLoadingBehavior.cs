using Microsoft.Xaml.Behaviors;
using MvvmScarletToolkit.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MvvmScarletToolkit
{
    // usage:
    // <i:Interaction.Behaviors>
    //    <mvvm:AsyncImageLoadingBehavior Source="{Binding Source}" Width="200" Height="200" />
    // </ i:Interaction.Behaviors>
    //
    // State, IsPending, IsLoading, IsLoaded and IsFailed are set by the behavior,
    // bind them with Mode=OneWayToSource to observe them from a view model.
    public sealed class AsyncImageLoadingBehavior : Behavior<Image>
    {
        public static Lazy<IImageService<BitmapSource>>? Loader { get; set; }

        private AsyncImageLoad? _load;

        protected override void OnAttached()
        {
            base.OnAttached();

            _load = new AsyncImageLoad(
                AssociatedObject,
                () => Loader,
                () => Source,
                () => AsyncImageLoad.GetSize(Width, Height),
                SetState);

            _load.Invalidate();
        }

        protected override void OnDetaching()
        {
            _load?.Detach();
            _load = null;

            base.OnDetaching();
        }

        private void SetState(ImageLoadState state)
        {
            SetCurrentValue(StateProperty, state);
            SetCurrentValue(IsPendingProperty, state == ImageLoadState.Pending);
            SetCurrentValue(IsLoadingProperty, state == ImageLoadState.Loading);
            SetCurrentValue(IsLoadedProperty, state == ImageLoadState.Loaded);
            SetCurrentValue(IsFailedProperty, state == ImageLoadState.Failed);
        }

        public Uri? Source
        {
            get { return (Uri?)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source),
            typeof(Uri),
            typeof(AsyncImageLoadingBehavior),
            new PropertyMetadata(null, OnPropertyChanged));

        public uint? Width
        {
            get { return (uint?)GetValue(WidthProperty); }
            set { SetValue(WidthProperty, value); }
        }

        public static readonly DependencyProperty WidthProperty = DependencyProperty.Register(
            nameof(Width),
            typeof(uint?),
            typeof(AsyncImageLoadingBehavior),
            new PropertyMetadata(null, OnPropertyChanged));

        public uint? Height
        {
            get { return (uint?)GetValue(HeightProperty); }
            set { SetValue(HeightProperty, value); }
        }

        public static readonly DependencyProperty HeightProperty = DependencyProperty.Register(
            nameof(Height),
            typeof(uint?),
            typeof(AsyncImageLoadingBehavior),
            new PropertyMetadata(null, OnPropertyChanged));

        /// <summary>
        /// Set by the behavior.
        /// </summary>
        public ImageLoadState State
        {
            get { return (ImageLoadState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
            nameof(State),
            typeof(ImageLoadState),
            typeof(AsyncImageLoadingBehavior),
            new PropertyMetadata(ImageLoadState.None));

        /// <summary>
        /// Set by the behavior, <see langword="true"/> while <see cref="State"/> is <see cref="ImageLoadState.Pending"/>.
        /// </summary>
        public bool IsPending
        {
            get { return (bool)GetValue(IsPendingProperty); }
            set { SetValue(IsPendingProperty, value); }
        }

        public static readonly DependencyProperty IsPendingProperty = DependencyProperty.Register(
            nameof(IsPending),
            typeof(bool),
            typeof(AsyncImageLoadingBehavior),
            new PropertyMetadata(false));

        /// <summary>
        /// Set by the behavior, <see langword="true"/> while <see cref="State"/> is <see cref="ImageLoadState.Loading"/>.
        /// </summary>
        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
            nameof(IsLoading),
            typeof(bool),
            typeof(AsyncImageLoadingBehavior),
            new PropertyMetadata(false));

        /// <summary>
        /// Set by the behavior, <see langword="true"/> while <see cref="State"/> is <see cref="ImageLoadState.Loaded"/>.
        /// </summary>
        public bool IsLoaded
        {
            get { return (bool)GetValue(IsLoadedProperty); }
            set { SetValue(IsLoadedProperty, value); }
        }

        public static readonly DependencyProperty IsLoadedProperty = DependencyProperty.Register(
            nameof(IsLoaded),
            typeof(bool),
            typeof(AsyncImageLoadingBehavior),
            new PropertyMetadata(false));

        /// <summary>
        /// Set by the behavior, <see langword="true"/> while <see cref="State"/> is <see cref="ImageLoadState.Failed"/>.
        /// </summary>
        public bool IsFailed
        {
            get { return (bool)GetValue(IsFailedProperty); }
            set { SetValue(IsFailedProperty, value); }
        }

        public static readonly DependencyProperty IsFailedProperty = DependencyProperty.Register(
            nameof(IsFailed),
            typeof(bool),
            typeof(AsyncImageLoadingBehavior),
            new PropertyMetadata(false));

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is AsyncImageLoadingBehavior behavior)
            {
                behavior._load?.Invalidate();
            }
        }
    }
}
