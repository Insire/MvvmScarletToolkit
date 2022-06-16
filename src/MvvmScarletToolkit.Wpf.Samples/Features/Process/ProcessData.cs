using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace MvvmScarletToolkit.Wpf.Samples
{
    [INotifyPropertyChanged]
    public sealed partial class ProcessData
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
