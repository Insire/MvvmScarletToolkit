using System.ComponentModel;

namespace MvvmScarletToolkit.Observables.Tests
{
    public interface ITestViewModel : INotifyPropertyChanged
    {
        string Property { get; set; }
    }
}
