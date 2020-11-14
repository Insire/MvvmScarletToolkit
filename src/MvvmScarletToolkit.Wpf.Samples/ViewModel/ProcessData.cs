using MvvmScarletToolkit.Observables;
using System;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class ProcessData : ObservableObject
    {
        public string Message { get; }
        public DateTime TimeStamp { get; }

        public ProcessData(string message, DateTime timeStamp)
        {
            Message = message;
            TimeStamp = timeStamp;
        }
    }
}
