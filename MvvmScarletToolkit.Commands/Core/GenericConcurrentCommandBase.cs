using MvvmScarletToolkit.Abstractions;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Commands
{
    /// <summary>
    /// Base implementation providing interface members for cancellation support and exposing current execution
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public abstract class GenericConcurrentCommandBase<TResult> : ConcurrentCommandBase, IExtendedAsyncCommand
    {
        [Bindable(true, BindingDirection.OneWay)]
        public Task Completion => Execution?.TaskCompletion ?? Task.CompletedTask;

        [Bindable(true, BindingDirection.OneWay)]
        public ICommand CancelCommand { get; protected set; }

        private bool _isBusy;
        [Bindable(true, BindingDirection.OneWay)]
        public bool IsBusy
        {
            get { return _isBusy; }
            protected set { SetValue(ref _isBusy, value); }
        }

        private NotifyTaskCompletion<TResult> _execution;
        [Bindable(true, BindingDirection.OneWay)]
        public NotifyTaskCompletion<TResult> Execution
        {
            get { return _execution; }
            protected set { SetValue(ref _execution, value); }
        }

        protected GenericConcurrentCommandBase(IScarletCommandManager commandManager)
            : base(commandManager)
        {
        }
    }
}
