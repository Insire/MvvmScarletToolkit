using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Windows;

namespace MvvmScarletToolkit
{
    public sealed class ExitService : IExitService
    {
        private readonly Application _app;
        private readonly ConcurrentStack<IBusinessViewModelListBase> _listViewModels;
        private readonly ConcurrentStack<IBusinessViewModelBase> _viewModels;

        public static IExitService Default { get; } = new ExitService(Application.Current);

        private ExitService()
        {
            _viewModels = new ConcurrentStack<IBusinessViewModelBase>();
            _listViewModels = new ConcurrentStack<IBusinessViewModelListBase>();
        }

        public ExitService(Application app)
            : this()
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));

            _app.Exit += App_Exit;
        }

        private async void App_Exit(object sender, ExitEventArgs e)
        {
            _app.Exit -= App_Exit;

            while (_viewModels.TryPop(out var viewmodel))
            {
                await viewmodel.Unload(CancellationToken.None);
            }

            while (_listViewModels.TryPop(out var viewmodel))
            {
                await viewmodel.Unload(CancellationToken.None);
            }
        }

        public void UnloadOnExit(IBusinessViewModelListBase viewModel)
        {
            _listViewModels.Push(viewModel);
        }

        public void UnloadOnExit(IBusinessViewModelBase viewModel)
        {
            _viewModels.Push(viewModel);
        }
    }
}
