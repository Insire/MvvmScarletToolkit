using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Core.Samples.Features.Process
{
    public sealed partial class ProcessData : ObservableObject
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
