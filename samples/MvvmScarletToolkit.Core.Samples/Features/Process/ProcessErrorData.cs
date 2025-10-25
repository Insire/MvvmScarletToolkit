using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Core.Samples.Features.Process
{
    public sealed partial class ProcessErrorData : ObservableObject
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
