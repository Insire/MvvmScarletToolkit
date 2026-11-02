using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Core.Samples.Features
{
    public sealed partial class FormViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string MaxLengthInput { get; set; }

        [ObservableProperty]
        public partial int MaxLength { get; set; }

        [ObservableProperty]
        public partial string Regex { get; set; }

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
