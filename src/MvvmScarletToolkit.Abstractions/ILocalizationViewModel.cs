using System;
using System.ComponentModel;

namespace MvvmScarletToolkit
{
    public interface ILocalizationViewModel : INotifyPropertyChanged, IDisposable
    {
        string Value { get; }
    }
}
