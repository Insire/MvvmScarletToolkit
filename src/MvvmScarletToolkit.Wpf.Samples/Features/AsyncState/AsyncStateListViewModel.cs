using Microsoft.Toolkit.Mvvm.Input;
using MvvmScarletToolkit.Observables;
using System;
using System.Threading;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class AsyncStateListViewModel : ViewModelListBase<AsyncStateViewModel>
    {
        private readonly Timer _timer;

        private string _filterText;
        public string FilterText
        {
            get { return _filterText; }
            set { SetProperty(ref _filterText, value); }
        }

        public Predicate<object> Filter { get; }

        public RelayCommand EnableGenerationCommand { get; }
        public RelayCommand DisableGenerationCommand { get; }

        public AsyncStateListViewModel(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            for (var i = 0; i < 10; i++)
            {
                AddUnchecked(new AsyncStateViewModel(commandBuilder)
                {
                    DisplayName = "Test " + i,
                });
            }

            SelectedItem = Items[0];

            Filter = ObjectFilter;

            _timer = new Timer(new TimerCallback(OnTimerElapsed), this, Timeout.InfiniteTimeSpan, TimeSpan.FromSeconds(1));

            EnableGenerationCommand = new RelayCommand(() => _timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(1)));
            DisableGenerationCommand = new RelayCommand(() => _timer.Change(Timeout.InfiniteTimeSpan, TimeSpan.FromSeconds(1)));
        }

        private static void OnTimerElapsed(object state)
        {
            if (state is AsyncStateListViewModel viewModel)
            {
                _ = viewModel.Dispatcher.Invoke(() =>
                {
                    var count = viewModel.Items.Count;
                    viewModel.Add(new AsyncStateViewModel(viewModel.CommandBuilder)
                    {
                        DisplayName = "New " + count
                    });
                });
            }
        }

        private bool ObjectFilter(object obj)
        {
            if (obj is not AsyncStateViewModel chat)
            {
                return true;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(FilterText))
                {
                    return true;
                }

                if (chat.DisplayName.IndexOf(FilterText, StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
