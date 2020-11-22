using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class ProcessErrorData : ObservableObject
    {
        public string Message { get; }
        public DateTime TimeStamp { get; }

        public ProcessErrorData(string message, DateTime timeStamp)
        {
            Message = message;
            TimeStamp = timeStamp;
        }
    }
}
