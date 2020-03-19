using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MvvmScarletToolkit
{
    public sealed class ScarletExitService : IExitService
    {
        private static readonly Lazy<ScarletExitService> _default = new Lazy<ScarletExitService>(() => new ScarletExitService(Application.Current, ScarletDispatcher.InternalDefault));
        public static IExitService Default => _default.Value;

        private readonly Application _app;
        private readonly ScarletDispatcher _scarletDispatcher;
        private readonly ConcurrentQueue<IVirtualizationViewModel> _viewModels;

        private Task _shutDown = Task.CompletedTask;

        public ScarletExitService(Application app, ScarletDispatcher dispatcher)
        {
            _viewModels = new ConcurrentQueue<IVirtualizationViewModel>();

            _app = app ?? throw new ArgumentNullException(nameof(app));
            _scarletDispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

            _app.Exit += OnAppExit;
            _app.SessionEnding += OnSessionEnding;
        }

        private void OnSessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
#if DEBUG
            Debug.WriteLine(nameof(ScarletExitService) + "." + nameof(OnSessionEnding));
#endif
            OnImmediateAppExit();
        }

        private void OnAppExit(object sender, ExitEventArgs e)
        {
#if DEBUG
            Debug.WriteLine(nameof(ScarletExitService) + "." + nameof(OnAppExit));
#endif
            OnImmediateAppExit();
        }

        private void OnImmediateAppExit()
        {
            _app.Exit -= OnAppExit;
            _app.SessionEnding -= OnSessionEnding;

            _scarletDispatcher.InvokeSynchronous = true;
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
