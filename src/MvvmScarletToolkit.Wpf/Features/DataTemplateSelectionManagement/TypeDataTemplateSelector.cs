using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// A <see cref="DataTemplateSelector"/> that will check <see cref="Templates"/> for a fitting DataTemplate before falling back to WPFs default resource lookup system
    /// </summary>
    public sealed class TypeDataTemplateSelector : DataTemplateSelector
    {
        private DataTemplateCollection? _templates;
        public DataTemplateCollection Templates => _templates ??= new DataTemplateCollection();

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            // e.g. a ContentPresenter without content
            if (item is null)
            {
                return null;
            }

            var type = item.GetType();

            return Templates.Find(type) ?? container.FindDataTemplateFor(type);
        }
    }
}
