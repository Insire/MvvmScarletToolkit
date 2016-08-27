using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    public class BusyToken : WeakReference, IDisposable
    {
        public bool Disposing { get; private set; }
        public bool Async { get; set; }

        public TaskScheduler TaskScheduler { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public TaskCreationOptions TaskCreationOptions { get; set; }

        private Action _action;
        public Action Action
        {
            get { return _action; }
            set
            {
                if (EqualityComparer<Action>.Default.Equals(value))
                    return;

                _action = value;

                if (Async)
                {
                    if (TaskScheduler != null)
                        Task = Task.Factory.StartNew(Action, CancellationToken, TaskCreationOptions, TaskScheduler);
                    else
                        Task = Task.Factory.StartNew(Action, CancellationToken);
                }
                else
                    Action();
            }
        }

        private Task _task;
        public Task Task
        {
            get { return _task; }
            private set
            {
                if (EqualityComparer<Task>.Default.Equals(value))
                    return;

                _task = value;

                _task.ContinueWith(t =>
                {
                    Dispose(true);
                });
            }
        }

        public BusyToken(BusyStack stack) : base(stack)
        {
            InitializeProperties();

            stack.Push(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!Disposing)
            {
                Disposing = true;

                var stack = Target as BusyStack;
                stack?.Pull();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void InitializeProperties()
        {
            CancellationTokenSource = new CancellationTokenSource();
            CancellationToken = CancellationTokenSource.Token;

            TaskCreationOptions = TaskCreationOptions.DenyChildAttach;
        }
    }
}
