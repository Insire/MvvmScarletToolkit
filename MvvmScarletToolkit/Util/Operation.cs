using System;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Util
{
    public class Operation : ObservableObject
    {
        private Action _action;

        private bool _isBusy;
        /// <summary>
        /// Indicates if there is an operation running.
        /// Modified by adding <see cref="BusyToken"/> to the <see cref="BusyStack"/> property
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            private set { SetValue(ref _isBusy, value); }
        }

        private BusyStack _busyStack;
        /// <summary>
        /// Provides IDisposable tokens for running async operations
        /// </summary>
        public BusyStack BusyStack
        {
            get { return _busyStack; }
            private set { SetValue(ref _busyStack, value); }
        }

        private Operation()
        {
            BusyStack = new BusyStack();
            BusyStack.CollectionChanged += BusyStackChanged;
        }

        public Operation(Action action) : this()
        {
            _action = action;
        }

        public virtual void BusyStackChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IsBusy = BusyStack.HasItems();
        }

        public Task Execute(CancellationTokenSource source = default(CancellationTokenSource))
        {
            return Task.Factory.StartNew(InternalExecute, source.Token);
        }

        public Task Execute(TaskCreationOptions options, TaskScheduler scheduler, CancellationTokenSource source = default(CancellationTokenSource))
        {
            return Task.Factory.StartNew(InternalExecute, source.Token, options, scheduler);
        }

        private void InternalExecute()
        {
            using (var token = BusyStack.GetToken())
                _action();
        }
    }

    public class Operation<T> : ObservableObject
    {
        private Action<T> _action;

        private bool _isBusy;
        /// <summary>
        /// Indicates if there is an operation running.
        /// Modified by adding <see cref="BusyToken"/> to the <see cref="BusyStack"/> property
        /// </summary>
        public bool IsBusy
        {
            get { return _isBusy; }
            private set { SetValue(ref _isBusy, value); }
        }

        private BusyStack _busyStack;
        /// <summary>
        /// Provides IDisposable tokens for running async operations
        /// </summary>
        public BusyStack BusyStack
        {
            get { return _busyStack; }
            private set { SetValue(ref _busyStack, value); }
        }

        private Operation()
        {
            BusyStack = new BusyStack();
            BusyStack.CollectionChanged += BusyStackChanged;
        }

        public Operation(Action<T> action) : this()
        {
            _action = action;
        }

        public virtual void BusyStackChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            IsBusy = BusyStack.HasItems();
        }

        public Task Execute(object parameter, CancellationTokenSource source = default(CancellationTokenSource))
        {
            return Task.Factory.StartNew(() => InternalExecute(parameter), source.Token);
        }

        public Task Execute(object parameter, TaskCreationOptions options, TaskScheduler scheduler, CancellationTokenSource source = default(CancellationTokenSource))
        {
            return Task.Factory.StartNew(() => InternalExecute(parameter), source.Token, options, scheduler);
        }

        private void InternalExecute(object parameter)
        {
            using (var token = BusyStack.GetToken())
                _action((T)parameter);
        }
    }
}
