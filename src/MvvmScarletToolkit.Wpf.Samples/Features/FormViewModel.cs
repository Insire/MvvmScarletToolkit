using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Samples.Features
{
    public sealed partial class FormViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _maxLengthInput;

        [ObservableProperty]
        private int _maxLength;

        [ObservableProperty]
        private string _regex;

        partial void OnMaxLengthInputChanged(string value)
        {
            if (int.TryParse(value, out var maxLength))
            {
                MaxLength = maxLength;
            }
        }

        public FormViewModel()
        {
            Regex = "^\\d+$";
            MaxLengthInput = "10";
        }
    }
}
