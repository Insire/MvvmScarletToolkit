using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit
{
    public class TypeDataTemplateSelector : DataTemplateSelector
    {
        private DataTemplateCollection _templates;
        public DataTemplateCollection Templates => _templates ?? (_templates = new DataTemplateCollection());

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var type = item.GetType();

            return Templates.FirstOrDefault(p => p.DataType.Equals(type)) ?? container.FindDataTemplateFor(type);
        }
    }
}
