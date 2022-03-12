using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MvvmScarletToolkit.Wpf
{
    public class ToastService : ObservableObject, IToastService
    {
        private static readonly Lazy<ToastService> _default = new Lazy<ToastService>(() => new ToastService(new ToastServiceConfiguration(), SynchronizationContext.Current!));

        public static IToastService Default => _default.Value;

        private readonly SourceList<IToast> _items;
        private readonly SynchronizationContext _synchronizationContext;
        private readonly IDisposable _itemsSubscription;
        private readonly IDisposable _countSubscription;
        private readonly IDisposable _removeSubscription;

        private Window? _host;
        private bool disposedValue;

        public string WindowStyleKey { get; }
        public int WindowOffset { get; }
        public TimeSpan WindowCloseDelay { get; }
        public TimeSpan ToastCloseDelay { get; }
        public TimeSpan ToastVisibleFor { get; }
        public Rect? Origin { get; set; }

        public IObservableCollection<IToast> Items { get; }

        /// <summary>
        /// will close and remove the toast immediately
        /// </summary>
        public ICommand DismissCommand { get; }

        public ToastService(ToastServiceConfiguration configuration, SynchronizationContext synchronizationContext)
        {
            _synchronizationContext = synchronizationContext ?? throw new ArgumentNullException(nameof(synchronizationContext));

            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            _items = new SourceList<IToast>();
            Items = new ObservableCollectionExtended<IToast>();

            WindowStyleKey = configuration.WindowStyleKey;
            WindowOffset = configuration.WindowOffset;

            WindowCloseDelay = configuration.WindowCloseDelay;
            ToastCloseDelay = configuration.ToastCloseDelay;
            ToastVisibleFor = configuration.ToastVisibleFor;

            DismissCommand = new AsyncRelayCommand<object>(DismissImpl);

            _itemsSubscription = _items
                .Connect()
                .ObserveOn(TaskPoolScheduler.Default)
                .DistinctUntilChanged()
                .ObserveOn(_synchronizationContext)
                .Bind(Items)
                .DisposeMany()
                .Subscribe();

            _countSubscription = _items.CountChanged
                .ObserveOn(TaskPoolScheduler.Default)
                .DistinctUntilChanged()
                .ObserveOn(_synchronizationContext)
                .Do(p =>
                {
                    if (p == 0)
                    {
                        var host = _host;
                        if (host is null)
                        {
                            return;
                        }

                        host.Visibility = Visibility.Hidden;
                    }
                })
                .ObserveOn(TaskPoolScheduler.Default)
                .Delay(WindowCloseDelay)
                .ObserveOn(_synchronizationContext)
                .Subscribe(_ =>
                {
                    if (_items.Count == 0)
                    {
                        var host = _host;
                        _host = null;

                        if (host is null)
                        {
                            return;
                        }

                        host.Close();
                    }
                });

            _removeSubscription = _items
                .Connect()
                .ObserveOn(TaskPoolScheduler.Default)
                .DistinctUntilChanged()
                .Filter(t => !t.IsPersistent)
                .Delay(ToastVisibleFor)
                .Subscribe(async changes =>
                {
                    foreach (var change in changes)
                    {
                        switch (change.Reason)
                        {
                            case ListChangeReason.Add:
                            case ListChangeReason.AddRange:
                                var toast = change.Item.Current;
                                await CloseToast(toast).ConfigureAwait(false);

                                break;

                            case ListChangeReason.Replace:
                            case ListChangeReason.Remove:
                            case ListChangeReason.RemoveRange:
                            case ListChangeReason.Clear:
                            case ListChangeReason.Refresh:
                            case ListChangeReason.Moved:
                                break;
                        }
                    }
                });
        }

        protected virtual Task DismissImpl(object? args)
        {
            if (args is IToast toast)
            {
                return CloseToast(toast);
            }

            return Task.CompletedTask;
        }

        protected virtual async Task CloseToast(IToast toast)
        {
            toast.IsRemoving = true;

            if (!toast.IsPersistent)
            {
                await Task.Delay(ToastCloseDelay).ConfigureAwait(false); // we need to wait a bit, since i'm not aware of a good way to animate removal
            }

            _items.Remove(toast);
        }

        public void Show(IToast toast)
        {
            _synchronizationContext.Post(new SendOrPostCallback(context => SetupHost((ToastService)context!)), this);

            _items.Add(toast);
        }

        private static void SetupHost(ToastService context)
        {
            // setup host window, if its missing
            if (context._host is null)
            {
                context._host = new Window()
                {
                    DataContext = context,
                };

                var obj = Application.Current.FindResource(context.WindowStyleKey);
                if (obj is Style style && style.TargetType == typeof(Window))
                {
                    context._host.Style = style;
                }

                context._host.Closing += OnHostClosing;
                context._host.Loaded += OnLoaded;
                context._host.Show();
            }

            Reposition(context._host, context.Origin, context.WindowOffset);

            if (!context._host.IsVisible)
            {
                context._host.Visibility = Visibility.Visible;
            }

            void OnLoaded(object sender, RoutedEventArgs e)
            {
                Reposition(context._host, context.Origin, context.WindowOffset);
            }

            void OnHostClosing(object? sender, EventArgs e)
            {
                context._host = null;

                if (sender is Window host)
                {
                    host.Closing -= OnHostClosing;
                }
            }
        }

        private static void Reposition(Window window, Rect? origin, int offset)
        {
            var area = origin ?? SystemParameters.WorkArea;

            // Display the toast at the top right of the area.
            window.Left = area.Right - window.Width - offset;
            window.Top = area.Top + offset;

            // set Height to screen height, so that animations are always smooth
            window.MinHeight = area.Height - (2 * offset);
            window.Height = window.MinHeight;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _itemsSubscription.Dispose();
                    _countSubscription.Dispose();
                    _removeSubscription.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
