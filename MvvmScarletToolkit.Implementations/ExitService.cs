using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MvvmScarletToolkit
{
    public sealed class ExitService : IExitService
    {
        private static readonly Lazy<ExitService> _default = new Lazy<ExitService>(() => new ExitService(Application.Current, ScarletDispatcher.InternalDefault));
        public static IExitService Default { get; } = _default.Value;

        private readonly Application _app;
        private readonly ScarletDispatcher _scarletDispatcher;
        private readonly ConcurrentQueue<IBusinessViewModelListBase> _listViewModels;
        private readonly ConcurrentQueue<IBusinessViewModelBase> _viewModels;

        private Task _shutDown = Task.CompletedTask;

        private ExitService()
        {
            _viewModels = new ConcurrentQueue<IBusinessViewModelBase>();
            _listViewModels = new ConcurrentQueue<IBusinessViewModelListBase>();
        }

        public ExitService(Application app, ScarletDispatcher dispatcher)
            : this()
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _scarletDispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));

            _app.Exit += OnAppExit;
            _app.SessionEnding += OnSessionEnding;
        }

        private void OnSessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            OnImmediateAppExit();
        }

        private void OnAppExit(object sender, ExitEventArgs e)
        {
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
            Debug.WriteLine("Started Unloading ViewModels...");

            var tasks = new List<Task>(_viewModels.Count + _listViewModels.Count);

            while (_viewModels.TryDequeue(out var viewmodel))
            {
                tasks.Add(viewmodel.Unload(CancellationToken.None));
            }

            while (_listViewModels.TryDequeue(out var viewmodel))
            {
                tasks.Add(viewmodel.Unload(CancellationToken.None));
            }

            return Task.WhenAll(tasks);
        }

        public Task ShutDown()
        {
            return _shutDown;
        }

        public void UnloadOnExit(IBusinessViewModelListBase viewModel)
        {
            _listViewModels.Enqueue(viewModel);
        }

        public void UnloadOnExit(IBusinessViewModelBase viewModel)
        {
            _viewModels.Enqueue(viewModel);
        }
    }
}
