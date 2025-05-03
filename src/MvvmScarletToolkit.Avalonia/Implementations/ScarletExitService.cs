using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public sealed class ScarletExitService : IExitService
    {
        private static readonly Lazy<ScarletExitService> _default = new Lazy<ScarletExitService>(() => new ScarletExitService(Application.Current!.ApplicationLifetime!, ScarletDispatcher.InternalDefault));
        public static IExitService Default => _default.Value;

        private readonly IClassicDesktopStyleApplicationLifetime _app;
        private readonly ScarletDispatcher _scarletDispatcher;
        private readonly ConcurrentQueue<IVirtualizationViewModel> _viewModels;

        private Task _shutDown = Task.CompletedTask;

        public ScarletExitService(IApplicationLifetime app, ScarletDispatcher dispatcher)
        {
            _viewModels = new ConcurrentQueue<IVirtualizationViewModel>();

            _app = app as IClassicDesktopStyleApplicationLifetime ?? throw new ArgumentNullException(nameof(app));
            _scarletDispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

            _app.Exit += OnExit;
            _app.ShutdownRequested += OnShutdownRequested;
        }

        private void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
        {
#if DEBUG
            Debug.WriteLine(nameof(ScarletExitService) + "." + nameof(OnShutdownRequested));
#endif
            OnImmediateAppExit();
        }

        private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
#if DEBUG
            Debug.WriteLine(nameof(ScarletExitService) + "." + nameof(OnExit));
#endif
            OnImmediateAppExit();
        }

        private void OnImmediateAppExit()
        {
            _app.Exit -= OnExit;
            _app.ShutdownRequested -= OnShutdownRequested;

            _scarletDispatcher.SetInvokeSynchronous(true);
            _shutDown = InternalShutDown();
        }

        private Task InternalShutDown()
        {
#if DEBUG
            Debug.WriteLine(nameof(ScarletExitService) + "." + nameof(InternalShutDown));
#endif

            var tasks = new List<Task>(_viewModels.Count);

            while (_viewModels.TryDequeue(out var viewmodel))
            {
                if (!viewmodel.IsLoaded)
                {
                    continue;
                }

                tasks.Add(viewmodel.Unload(CancellationToken.None));
            }

            return Task.WhenAll(tasks);
        }

        public Task ShutDown()
        {
            return _shutDown;
        }

        public void UnloadOnExit(IVirtualizationViewModel viewModel)
        {
            _viewModels.Enqueue(viewModel);
        }
    }
}
