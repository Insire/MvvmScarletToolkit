using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    public sealed class NotifyTaskCompletion : INotifyPropertyChanged
    {
        private static readonly Lazy<NotifyTaskCompletion> _completed = new Lazy<NotifyTaskCompletion>(() => new NotifyTaskCompletion(Task.CompletedTask));
        public static NotifyTaskCompletion Completed => _completed.Value;

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

        public AggregateException Exception => Task.Exception.Flatten();
        public Exception? InnerException => Exception?.InnerException;

        public Task Task { get; }
        public Task TaskCompletion { get; }

        public NotifyTaskCompletion(in Task task)
        {
            Task = task ?? throw new ArgumentNullException(nameof(task));

            TaskCompletion = task == Task.CompletedTask ? Task.CompletedTask : WatchTaskAsync(task);
        }

        private async Task WatchTaskAsync(Task task)
        {
            try
            {
                // dont configureawait(false) here since we want to raise OnPropertyChanged on the ui thread if possible
                // but we are not going to enforce that

#pragma warning disable RCS1090 // Call 'ConfigureAwait(false)'.
                await task;
#pragma warning restore RCS1090 // Call 'ConfigureAwait(false)'.
            }
            // no need to catch, since we capture the exception through the property task
            // and we dont want to take down the whole application if the developer didnt add any exception handling
#if DEBUG
            catch (TaskCanceledException)
            {
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

#else
            catch
            {
            }
#endif
            finally
            {
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(IsRunning));
                OnPropertyChanged(nameof(IsCompleted));

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
        }

        private void OnPropertyChanged([CallerMemberName] in string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
