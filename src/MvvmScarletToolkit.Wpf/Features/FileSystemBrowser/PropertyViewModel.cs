using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using System.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Features.FileSystemBrowser
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

        public static void AddUpdateOrUpdateCache(SourceCache<PropertyViewModel, string> cache, int sequence, string key, string value)
        {
            var found = cache.Lookup(key);
            if (found.HasValue)
            {
                found.Value.Value = value;
            }
            else
            {
                cache.AddOrUpdate(Create(key, value, sequence));
            }
        }

        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string Key { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string Value { get; set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial string DisplayName { get; private set; }
        [ObservableProperty, Bindable(true, BindingDirection.OneWay)] public partial int Sequence { get; private set; }
    }
}
