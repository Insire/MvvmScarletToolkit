using System;
using System.ComponentModel;

namespace MvvmScarletToolkit.Samples
{
    public sealed class ProcessData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Message { get; }
        public DateTime TimeStamp { get; }

        public ProcessData(string message, DateTime timeStamp)
        {
            Message = message;
            TimeStamp = timeStamp;
        }
    }
}
