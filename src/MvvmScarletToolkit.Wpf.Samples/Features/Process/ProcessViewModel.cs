using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed partial class ProcessViewModel : ViewModelBase
    {
        private readonly ConcurrentQueue<ProcessData> _outputQueue;
        private readonly ConcurrentQueue<ProcessErrorData> _errorQueue;

        private readonly ObservableCollection<ProcessData> _output;
        private readonly ObservableCollection<ProcessErrorData> _errors;

        private readonly DispatcherTimer _timer;
        private readonly ConcurrentCommandBase _startCommand;

        [ObservableProperty]
        private string _args;

        [ObservableProperty]
        private string _filePath;

        [ObservableProperty]
        private string _workingDirectory;

        public ICommand StartCommand => _startCommand;

        public ReadOnlyObservableCollection<ProcessData> Output { get; }
        public ReadOnlyObservableCollection<ProcessErrorData> Errors { get; }

        public ProcessViewModel(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            _workingDirectory = ".";
            _filePath = "cmd.exe";
            _args = "/c nslookup invalidname";

            _outputQueue = new ConcurrentQueue<ProcessData>();
            _errorQueue = new ConcurrentQueue<ProcessErrorData>();

            _output = new ObservableCollection<ProcessData>();
            _errors = new ObservableCollection<ProcessErrorData>();

            Output = new ReadOnlyObservableCollection<ProcessData>(_output);
            Errors = new ReadOnlyObservableCollection<ProcessErrorData>(_errors);

            _timer = new DispatcherTimer();
            _timer.Tick += OnTimerTick;
            _timer.Interval = TimeSpan.FromMilliseconds(500);

            _startCommand = commandBuilder.Create(Start, CanStart)
                .WithAsyncCancellation()
                .WithBusyNotification(BusyStack)
                .Build();
        }

        private async void OnTimerTick(object sender, EventArgs e)
        {
            var count = Math.Max(_outputQueue.Count, 10);
            while (count > 0)
            {
                if (_outputQueue.TryDequeue(out var data))
                {
                    await Dispatcher.Invoke(() => _output.Add(data)).ConfigureAwait(false);
                }

                count--;
            }

            count = Math.Max(_errorQueue.Count, 10);
            while (count > 0)
            {
                if (_errorQueue.TryDequeue(out var error))
                {
                    await Dispatcher.Invoke(() => _errors.Add(error)).ConfigureAwait(false);
                }

                count--;
            }
        }

        private Task Start(CancellationToken token)
        {
            return Task.Run(() => StartInternal(token), token);
        }

        private async Task StartInternal(CancellationToken token)
        {
            await Dispatcher.Invoke(() => _errors.Clear()).ConfigureAwait(false);
            await Dispatcher.Invoke(() => _output.Clear()).ConfigureAwait(false);

            var tcs = new TaskCompletionSource<object>();

            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo(FilePath, Args)
                {
                    ErrorDialog = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = WorkingDirectory,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                process.OutputDataReceived += OnOutputDataReceived;
                process.ErrorDataReceived += OnErrorDataReceived;
                process.Exited += OnExited;
                process.EnableRaisingEvents = true;

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                _timer.Start();

                using (token.Register(() => Task.Run(() => tcs.TrySetCanceled())))
                {
                    await tcs.Task.ConfigureAwait(false);
                }

                void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
                {
                    var data = e.Data?.Trim();
                    if (data?.Length > 0)
                    {
                        _outputQueue.Enqueue(new ProcessData(data, DateTime.UtcNow));
                    }
                }

                void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
                {
                    var data = e.Data?.Trim();
                    if (data?.Length > 0)
                    {
                        _errorQueue.Enqueue(new ProcessErrorData(data, DateTime.UtcNow));
                    }
                }

                void OnExited(object sender, EventArgs e)
                {
                    process.Exited -= OnExited;
                    process.OutputDataReceived -= OnOutputDataReceived;
                    process.ErrorDataReceived -= OnErrorDataReceived;

                    tcs.TrySetResult(null);
                }
            }
        }

        private bool CanStart()
        {
            return !_startCommand.IsBusy
                && FilePath?.Length > 0;
        }
    }
}
