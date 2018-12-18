using System;
using System.Collections;
using System.Collections.Generic;
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

    public sealed class DataTemplateCollection : IEnumerable<DataTemplateContainer>, IList
    {
        private readonly List<DataTemplateContainer> _templates;

        public int Count => _templates.Count;
        public bool IsSynchronized => false;

        public object SyncRoot { get; }
        public bool IsReadOnly { get; }
        public bool IsFixedSize { get; }

        public object this[int index]
        {
            get { return _templates[index]; }
            set { _templates[index] = value as DataTemplateContainer; }
        }

        public DataTemplateCollection()
        {
            _templates = new List<DataTemplateContainer>();
            SyncRoot = new object();
        }

        public void CopyTo(Array array, int index)
        {
            //_templates.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return _templates.GetEnumerator();
        }

        IEnumerator<DataTemplateContainer> IEnumerable<DataTemplateContainer>.GetEnumerator()
        {
            return _templates.GetEnumerator();
        }

        public int Add(object value)
        {
            _templates.Add(value as DataTemplateContainer);

            return _templates.Count - 1;
        }

        public bool Contains(object value)
        {
            return _templates.Contains(value as DataTemplateContainer);
        }

        public void Clear()
        {
            _templates.Clear();
        }

        public int IndexOf(object value)
        {
            return _templates.IndexOf(value as DataTemplateContainer);
        }

        public void Insert(int index, object value)
        {
            _templates.Insert(index, value as DataTemplateContainer);
        }

        public void Remove(object value)
        {
            _templates.Remove(value as DataTemplateContainer);
        }

        public void RemoveAt(int index)
        {
            _templates.RemoveAt(index);
        }
    }
}
