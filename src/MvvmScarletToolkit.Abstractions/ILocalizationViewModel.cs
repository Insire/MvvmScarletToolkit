using System;
using System.ComponentModel;

namespace MvvmScarletToolkit.Abstractions
{
    public interface ILocalizationViewModel : INotifyPropertyChanged, IDisposable
    {
        object Value { get; }
    }
}
