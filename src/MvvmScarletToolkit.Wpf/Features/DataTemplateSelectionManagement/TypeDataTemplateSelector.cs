using System.Linq;
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
            var type = item.GetType();

            return Templates.FirstOrDefault(p => p.DataType.Equals(type)) ?? container.FindDataTemplateFor(type);
        }
    }
}
