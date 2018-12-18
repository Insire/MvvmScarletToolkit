using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit.FileSystemBrowser
{
    public class TypeDataTemplateSelector : DataTemplateSelector
    {
        private readonly Dictionary<Type, DataTemplate> _lookup;

        public DataTemplateCollection Templates { get; set; }

        public TypeDataTemplateSelector()
        {
            _lookup = new Dictionary<Type, DataTemplate>();
            Templates = new DataTemplateCollection();
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var type = item.GetType();

            if (_lookup.TryGetValue(type, out var result))
            {
                return result;
            }

            result = Templates.FirstOrDefault(p => p.DataTemplate.DataType.Equals(type))?.DataTemplate ?? base.SelectTemplate(item, container);

            if (!_lookup.ContainsKey(type))
            {
                _lookup.Add(type, result);
            }

            return result;
        }
    }
}
