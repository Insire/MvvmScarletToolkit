using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public sealed class ObservableBusyViewModel : ObservableObject, IObservable<bool>
    {
        private readonly ObservableBusyStack _observableBusyStack;

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            private set { SetValue(ref _isBusy, value); }
        }

        public ConcurrentCommandBase BeBusyCommand { get; }

        public ObservableBusyViewModel(ICommandBuilder commandBuilder, IScarletDispatcher dispatcher)
        {
            _observableBusyStack = new ObservableBusyStack(hasItems => { IsBusy = hasItems; Debug.WriteLine("ObservableBusyViewModel is busy: " + hasItems); }, dispatcher);
            BeBusyCommand = commandBuilder.Create(BeBusyInternal, () => !IsBusy);
        }

        private async Task BeBusyInternal(CancellationToken token)
        {
            using (_observableBusyStack.GetToken())
            {
                await Task.Delay(250, token).ConfigureAwait(false);
            }
        }

        public IDisposable Subscribe(IObserver<bool> observer)
        {
            return _observableBusyStack.Subscribe(observer);
        }
    }
}
