using Xamarin.Forms;

namespace MvvmScarletToolkit
{
    public sealed class BindingProxy : Element
    {
        public static readonly BindableProperty DataProperty = BindableProperty.Create(
            nameof(Data),
            typeof(object),
            typeof(BindingProxy),
            default);

        public object Data
        {
            get { return GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }
    }
}
