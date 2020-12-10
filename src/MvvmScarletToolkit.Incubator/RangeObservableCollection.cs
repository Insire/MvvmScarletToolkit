using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MvvmScarletToolkit
{
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        private readonly SynchronizationContext _synchronizationContext = SynchronizationContext.Current;

        private bool _suppressNotification;

        public RangeObservableCollection()
            : base()
        {
        }

        public RangeObservableCollection(IEnumerable<T> items)
            : base(items)

        {
        }

        public RangeObservableCollection(List<T> items)
            : base(items)

        {
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_suppressNotification)
            {
                return;
            }

            if (SynchronizationContext.Current.Equals(_synchronizationContext))
            {
                // Execute the CollectionChanged event on the current thread
                RaiseCollectionChanged(e);
            }
            else
            {
                // Raises the CollectionChanged event on the creator thread
                _synchronizationContext.Send(RaiseCollectionChanged, e);
            }
        }

        private void RaiseCollectionChanged(object param)
        {
            // We are in the creator thread, call the base implementation directly
            base.OnCollectionChanged((NotifyCollectionChangedEventArgs)param);
        }

        public virtual void AddRange(IEnumerable<T> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            _suppressNotification = true;

            foreach (var item in items)
            {
                Add(item);
            }

            _suppressNotification = false;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public virtual void RemoveRange(IEnumerable<T> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            _suppressNotification = true;

            foreach (var item in items)
            {
                Remove(item);
            }

            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public virtual void RemoveRange(IList items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            _suppressNotification = true;

            foreach (var item in items.Cast<T>())
            {
                Remove(item);
            }

            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public virtual void RemoveRange(IList<T> items)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            _suppressNotification = true;

            foreach (var item in items)
            {
                Remove(item);
            }

            _suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (SynchronizationContext.Current.Equals(_synchronizationContext))
            {
                // Execute the PropertyChanged event on the current thread
                OnPropertyChanged(e);
            }
            else
            {
                // Raises the PropertyChanged event on the creator thread
                _synchronizationContext.Send(new SendOrPostCallback((object _) => OnPropertyChanged(e)), e);
            }
        }
    }
}
