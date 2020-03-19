using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// Collection of DataTemplates, limited to one entry per <see cref="DataTemplate.DataType"/>
    /// </summary>
    public sealed class DataTemplateCollection : Collection<DataTemplate>
    {
        public DataTemplate? Find(Type type)
        {
            foreach (var candidate in Items)
            {
                if (Equals(candidate.DataType, type))
                {
                    return candidate;
                }
            }

            return null;
        }

        protected override void InsertItem(int index, DataTemplate item)
        {
            ValidateItem(item);
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, DataTemplate item)
        {
            ValidateItem(item);
            base.SetItem(index, item);
        }

        private void ValidateItem(DataTemplate item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (item.DataType is null)
            {
                throw new InvalidOperationException("DataTemplate.DataType cannot be null.");
            }

            if (item.DataType is Type type)
            {
                if (Find(type) != null)
                {
                    throw new InvalidOperationException("Template already added for type.");
                }
            }
            else
            {
                Debug.WriteLine($"{item.DataType.GetType().Name} is not supported in a {nameof(DataTemplateCollection)}");
            }
        }
    }
}
