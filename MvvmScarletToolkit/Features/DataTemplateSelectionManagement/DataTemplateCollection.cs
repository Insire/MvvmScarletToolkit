using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace MvvmScarletToolkit
{
    public sealed class DataTemplateCollection : Collection<DataTemplate>
    {
        public DataTemplate Find(Type type)
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

            if (Find((Type)item.DataType) != null)
            {
                throw new InvalidOperationException("Template already added for type.");
            }
        }
    }
}
