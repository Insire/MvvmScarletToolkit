using System.ComponentModel;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    /// <summary>
    /// Base implementation providing interface members for cancellation support and exposing current execution
    /// </summary>
    public abstract class GenericConcurrentCommandBase : ConcurrentCommandBase
    {
        [Bindable(true, BindingDirection.OneWay)]
        public override sealed Task Completion => Execution.TaskCompletion;

        private NotifyTaskCompletion _execution;
        [Bindable(true, BindingDirection.OneWay)]
        public NotifyTaskCompletion Execution
        {
            get { return _execution; }
            protected set
            {
                if (SetValue(ref _execution, value))
                {
                    OnPropertyChanged(nameof(Completion));
                }
            }
        }

        protected GenericConcurrentCommandBase(in IScarletCommandManager commandManager)
            : base(commandManager)
        {
            _execution = NotifyTaskCompletion.Completed;
        }
    }
}
