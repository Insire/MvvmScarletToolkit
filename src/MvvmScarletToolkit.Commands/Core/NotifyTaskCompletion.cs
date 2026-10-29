using System.ComponentModel;

namespace MvvmScarletToolkit.Commands
{
    public sealed class NotifyTaskCompletion : INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs _statusChanged = new PropertyChangedEventArgs(nameof(Status));
        private static readonly PropertyChangedEventArgs _isRunningChanged = new PropertyChangedEventArgs(nameof(IsRunning));
        private static readonly PropertyChangedEventArgs _isCompletedChanged = new PropertyChangedEventArgs(nameof(IsCompleted));
        private static readonly PropertyChangedEventArgs _isCanceledChanged = new PropertyChangedEventArgs(nameof(IsCanceled));
        private static readonly PropertyChangedEventArgs _isFaultedChanged = new PropertyChangedEventArgs(nameof(IsFaulted));
        private static readonly PropertyChangedEventArgs _exceptionChanged = new PropertyChangedEventArgs(nameof(Exception));
        private static readonly PropertyChangedEventArgs _innerExceptionChanged = new PropertyChangedEventArgs(nameof(InnerException));
        private static readonly PropertyChangedEventArgs _errorMessageChanged = new PropertyChangedEventArgs(nameof(ErrorMessage));
        private static readonly PropertyChangedEventArgs _isSuccessfullyCompletedChanged = new PropertyChangedEventArgs(nameof(IsSuccessfullyCompleted));

        private readonly IScarletExceptionHandler _exceptionHandler;

        private AggregateException? _exception;

        public static NotifyTaskCompletion Completed { get; } = new NotifyTaskCompletion(Task.CompletedTask, IgnoreExceptionHandler.Default);

        public event PropertyChangedEventHandler? PropertyChanged;

        [Bindable(true, BindingDirection.OneWay)]
        public TaskStatus Status => Task.Status;

        [Bindable(true, BindingDirection.OneWay)]
        public bool IsRunning => !Task.IsCompleted;

        [Bindable(true, BindingDirection.OneWay)]
        public bool IsCompleted => Task.IsCompleted;

        [Bindable(true, BindingDirection.OneWay)]
        public bool IsCanceled => Task.IsCanceled;

        [Bindable(true, BindingDirection.OneWay)]
        public bool IsFaulted => Task.IsFaulted;

        [Bindable(true, BindingDirection.OneWay)]
        public bool IsSuccessfullyCompleted => Task.Status == TaskStatus.RanToCompletion;

        [Bindable(true, BindingDirection.OneWay)]
        public string ErrorMessage => InnerException?.Message ?? string.Empty;

        // flattening allocates a new AggregateException, so only do it once
        public AggregateException? Exception => _exception ??= Task.Exception?.Flatten();
        public Exception? InnerException => Exception?.InnerException;

        public Task Task { get; }

        public Task TaskCompletion { get; }

        public NotifyTaskCompletion(in Task task, in IScarletExceptionHandler exceptionHandler)
        {
            Task = task ?? throw new ArgumentNullException(nameof(task));
            _exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
            TaskCompletion = task == Task.CompletedTask ? Task.CompletedTask : WatchTaskAsync(task);
        }

        private async Task WatchTaskAsync(Task task)
        {
            try
            {
                await task;
            }
            catch (OperationCanceledException) when (task.IsCanceled)
            {
                // cancellation is an expected outcome and is reported via IsCanceled instead of the exception handler
            }
            catch (Exception ex)
            {
                await _exceptionHandler.Handle(ex);
            }
            finally
            {
                OnPropertyChanged(_statusChanged);
                OnPropertyChanged(_isRunningChanged);
                OnPropertyChanged(_isCompletedChanged);

                if (task.IsCanceled)
                {
                    OnPropertyChanged(_isCanceledChanged);
                }
                else if (task.IsFaulted)
                {
                    OnPropertyChanged(_isFaultedChanged);
                    OnPropertyChanged(_exceptionChanged);
                    OnPropertyChanged(_innerExceptionChanged);
                    OnPropertyChanged(_errorMessageChanged);
                }
                else
                {
                    OnPropertyChanged(_isSuccessfullyCompletedChanged);
                }
            }
        }

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
