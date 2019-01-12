using MvvmScarletToolkit.Abstractions;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    /// <summary>
    /// Base implementation providing interface members for cancellation support and exposing current execution
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public abstract class GenericConcurrentCommandBase<TResult> : ConcurrentCommandBase
    {
        [Bindable(true, BindingDirection.OneWay)]
        public override Task Completion => Execution?.TaskCompletion ?? Task.CompletedTask;

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
