using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace MvvmScarletToolkit.Wpf.FileSystemBrowser
{
    public sealed partial class PropertyViewModel : ObservableObject
    {
        public static PropertyViewModel Create(string key, string value, int sequence)
        {
            return Create(key, value, key, sequence);
        }

        public static PropertyViewModel Create(string key, string value, string displayName, int sequence)
        {
            return new PropertyViewModel()
            {
                Key = key,
                Value = value,
                DisplayName = displayName,
                Sequence = sequence
            };
        }

        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string Key { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string Value { get; set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string DisplayName { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial int Sequence { get; private set; }
    }
}
