using MvvmScarletToolkit.Abstractions;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MvvmScarletToolkit.Commands
{
    internal abstract class GenericAsyncCommandBase<TResult> : AsyncCommandBase, IExtendedAsyncCommand
    {
        [Bindable(true, BindingDirection.OneWay)]
        public ICommand CancelCommand { get; protected set; }

        private NotifyTaskCompletion<TResult> _execution;
        [Bindable(true, BindingDirection.OneWay)]
        public NotifyTaskCompletion<TResult> Execution
        {
            get { return _execution; }
            protected set { SetValue(ref _execution, value); }
        }

        public Task Completion => Execution?.TaskCompletion ?? Task.CompletedTask;
    }
}
