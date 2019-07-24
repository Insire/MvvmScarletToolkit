using System.Windows;

namespace MvvmScarletToolkit
{
    public sealed class DataTemplateContainer : Freezable
    {
        public DataTemplate DataTemplate
        {
            get { return (DataTemplate)GetValue(DataTemplateProperty); }
            set { SetValue(DataTemplateProperty, value); }
        }

        /// <summary>Identifies the <see cref="DataTemplate"/> dependency property.</summary>
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
