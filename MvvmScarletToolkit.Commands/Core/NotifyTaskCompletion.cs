using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    public sealed class NotifyTaskCompletion : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [Bindable(true, BindingDirection.OneWay)]
        public TaskStatus Status => Task.Status;

        [Bindable(true, BindingDirection.OneWay)]
        public bool IsCompleted => Task.IsCompleted;

        [Bindable(true, BindingDirection.OneWay)]
        public bool IsNotCompleted => !Task.IsCompleted;

        [Bindable(true, BindingDirection.OneWay)]
        public bool IsCanceled => Task.IsCanceled;

        [Bindable(true, BindingDirection.OneWay)]
        public bool IsFaulted => Task.IsFaulted;

        [Bindable(true, BindingDirection.OneWay)]
        public bool IsSuccessfullyCompleted => Task.Status == TaskStatus.RanToCompletion;

        [Bindable(true, BindingDirection.OneWay)]
        public string ErrorMessage => InnerException?.Message ?? string.Empty;

        public AggregateException Exception => Task.Exception.Flatten();
        public Exception InnerException => Exception?.InnerException;

        public Task Task { get; }
        public Task TaskCompletion { get; }

        public NotifyTaskCompletion(Task task)
        {
            Task = task ?? throw new ArgumentNullException(nameof(task));
            TaskCompletion = WatchTaskAsync(task);
        }

        private async Task WatchTaskAsync(Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch
            {
                // no need to catch, since we capture the exception through the property task
            }

            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(IsCompleted));
            OnPropertyChanged(nameof(IsNotCompleted));

            if (task.IsCanceled)
            {
                OnPropertyChanged(nameof(IsCanceled));
            }
            else if (task.IsFaulted)
            {
                OnPropertyChanged(nameof(IsFaulted));
                OnPropertyChanged(nameof(Exception));
                OnPropertyChanged(nameof(InnerException));
                OnPropertyChanged(nameof(ErrorMessage));
            }
            else
            {
                OnPropertyChanged(nameof(IsSuccessfullyCompleted));
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
