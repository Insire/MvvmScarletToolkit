using System;
using System.ComponentModel;

namespace MvvmScarletToolkit.Samples
{
    public sealed class ProcessErrorData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Message { get; }
        public DateTime TimeStamp { get; }

        public ProcessErrorData(string message, DateTime timeStamp)
        {
            Message = message;
            TimeStamp = timeStamp;
        }
    }
}
