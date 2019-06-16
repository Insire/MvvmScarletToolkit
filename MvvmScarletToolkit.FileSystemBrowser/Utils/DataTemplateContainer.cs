using System.Windows;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    public sealed class DataTemplateContainer : Freezable
    {
        public DataTemplate DataTemplate
        {
            get { return (DataTemplate)GetValue(DataTemplateProperty); }
            set { SetValue(DataTemplateProperty, value); }
        }

        public static readonly DependencyProperty DataTemplateProperty = DependencyProperty.Register(
            nameof(DataTemplate),
            typeof(DataTemplate),
            typeof(DataTemplateContainer),
            new PropertyMetadata(default(DataTemplate)));

        protected override Freezable CreateInstanceCore()
        {
            return new DataTemplateContainer();
        }
    }
}
