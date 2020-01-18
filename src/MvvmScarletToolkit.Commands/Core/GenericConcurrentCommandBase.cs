using MvvmScarletToolkit.Abstractions;
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
        public override Task Completion => Execution?.TaskCompletion ?? Task.CompletedTask;

        private NotifyTaskCompletion? _execution;
        [Bindable(true, BindingDirection.OneWay)]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("PropertyChangedAnalyzers.PropertyChanged", "INPC005:Check if value is different before notifying.",
            Justification = "Since Completion is a forwarded property of Execution, it is guaranteed to change, when the source property changed")]
        public NotifyTaskCompletion? Execution
        {
            get { return _execution; }
            protected set { SetValue(ref _execution, value, OnChanged: () => OnPropertyChanged(nameof(Completion))); }
        }

        protected GenericConcurrentCommandBase(IScarletCommandManager commandManager)
            : base(commandManager)
        {
        }
    }
}
