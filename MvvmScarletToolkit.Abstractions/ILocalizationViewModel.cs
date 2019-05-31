using System.ComponentModel;

namespace MvvmScarletToolkit.Abstractions
{
    public interface ILocalizationViewModel : INotifyPropertyChanged
    {
        object Value { get; }
    }
}
