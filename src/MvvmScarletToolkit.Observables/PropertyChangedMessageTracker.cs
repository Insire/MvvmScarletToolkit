using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System.ComponentModel;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// ChangeTracker listening for <see cref="PropertyChangedMessage{T}"/> from an <see cref="IMessenger"/>
    /// </summary>
    public sealed class PropertyChangedMessageTracker : IToolkitChangeTracker
    {
        private readonly Dictionary<object, Changes> _trackedInstances;
        private readonly IMessenger _messenger;

        private bool _disposedValue;
        private int _skipChangesCount;

        public PropertyChangedMessageTracker()
            : this(WeakReferenceMessenger.Default)
        {
        }

        public PropertyChangedMessageTracker(IMessenger messenger)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _trackedInstances = new Dictionary<object, Changes>();
        }

        /// <inheritdoc/>
        public void Track<TInstance, TPropertyType>(TInstance instance)
            where TInstance : class, INotifyPropertyChanged
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(PropertyChangedMessageTracker));
            }

            // tracking the same instance for another property type must not discard its recorded changes
            if (!_trackedInstances.ContainsKey(instance))
            {
                _trackedInstances.Add(instance, new Changes());
            }

            // the messenger only allows a single registration per recipient and message type
            if (_messenger.IsRegistered<PropertyChangedMessage<TPropertyType>>(this))
            {
                return;
            }

            _messenger.Register<PropertyChangedMessageTracker, PropertyChangedMessage<TPropertyType>>(this, (r, m) =>
            {
                if (r._skipChangesCount != 0)
                {
                    return;
                }

                if (r._trackedInstances.TryGetValue(m.Sender, out var changes))
                {
                    changes.Upsert(m);
                }
            });
        }

        /// <inheritdoc/>
        public void StopAllTracking()
        {
            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(PropertyChangedMessageTracker));
            }

            _trackedInstances.Clear();
        }

        /// <inheritdoc/>
        public void StopTracking<T>(T instance)
            where T : class, INotifyPropertyChanged

        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(PropertyChangedMessageTracker));
            }

            _trackedInstances.Remove(instance);
        }

        public bool TryGetChangeFor<T>(T instance, string propertyName, out IChange? change)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(PropertyChangedMessageTracker));
            }

            if (_trackedInstances.TryGetValue(instance, out var changes))
            {
                return changes.TryGetValue(propertyName, out change);
            }

            change = null;
            return false;
        }

        public bool TryGetChangeFor<TViewModel, TPropertyType>(TViewModel instance, string propertyName, out IChange<TPropertyType>? change)
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(PropertyChangedMessageTracker));
            }

            if (_trackedInstances.TryGetValue(instance, out var changes))
            {
                return changes.TryGetValue(propertyName, out change);
            }

            change = null;
            return false;
        }

        /// <inheritdoc/>
        public bool HasChanges<T>(T instance)
              where T : class, INotifyPropertyChanged
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(PropertyChangedMessageTracker));
            }

            if (_trackedInstances.TryGetValue(instance, out var stack))
            {
                return stack.IsChanged;
            }

            return false;
        }

        /// <inheritdoc/>
        public bool HasChanges()
        {
            return _trackedInstances.Values.Any(p => p.IsChanged);
        }

        /// <inheritdoc/>
        public int CountChanges<T>(T instance)
            where T : class, INotifyPropertyChanged
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(PropertyChangedMessageTracker));
            }

            if (_trackedInstances.TryGetValue(instance, out var changes))
            {
                return changes.Count;
            }
            else
            {
                return 0;
            }
        }

        /// <inheritdoc/>
        public void ClearAllChanges()
        {
            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(PropertyChangedMessageTracker));
            }

            foreach (var changes in _trackedInstances.Values)
            {
                changes.Clear();
            }
        }

        /// <inheritdoc/>
        public void ClearChanges<T>(T instance)
            where T : class, INotifyPropertyChanged
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(PropertyChangedMessageTracker));
            }

            if (_trackedInstances.TryGetValue(instance, out var changes))
            {
                changes.Clear();
            }
        }

        /// <inheritdoc/>
        public IDisposable SuppressAllChanges()
        {
            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(PropertyChangedMessageTracker));
            }

            return new IgnoreGlobalChanges(this);
        }

        /// <inheritdoc/>
        public IDisposable SuppressChanges<T>(T instance)
            where T : class, INotifyPropertyChanged
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(PropertyChangedMessageTracker));
            }

            if (_trackedInstances.TryGetValue(instance, out var changes))
            {
                return changes.SkipChanges();
            }

            throw new InvalidOperationException("The provided instance is not being tracked"); // TODO ? either this throws or we return a nullable disposeable
        }

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _messenger.UnregisterAll(this);
                    _trackedInstances.Clear();
                }

                _disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private sealed class IgnoreGlobalChanges : IDisposable
        {
            private readonly PropertyChangedMessageTracker _instance;

            private int _disposed;

            public IgnoreGlobalChanges(PropertyChangedMessageTracker instance)
            {
                _instance = instance;
                Interlocked.Increment(ref _instance._skipChangesCount);
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 0)
                {
                    Interlocked.Decrement(ref _instance._skipChangesCount);
                }
            }
        }

        private sealed class Changes
        {
            private readonly Dictionary<string, Change> _changes;

            private int _suppressedState;

            public int Count
            {
                get
                {
                    var count = 0;
                    foreach (var change in _changes.Values)
                    {
                        if (change.IsActualChange)
                        {
                            count++;
                        }
                    }

                    return count;
                }
            }

            public bool IsChanged
            {
                get
                {
                    foreach (var change in _changes.Values)
                    {
                        if (change.IsActualChange)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }

            public bool ShouldSuppressChanges => _suppressedState != 0;

            public Changes()
            {
                _changes = new Dictionary<string, Change>();
            }

            public bool TryGetValue(string key, out IChange? value)
            {
                if (_changes.TryGetValue(key, out var change))
                {
                    value = change;
                    return true;
                }

                value = null;
                return false;
            }

            public bool TryGetValue<TProperty>(string key, out IChange<TProperty>? value)
            {
                if (_changes.TryGetValue(key, out var change) && change is IChange<TProperty> typedChange)
                {
                    value = typedChange;
                    return true;
                }

                value = null;
                return false;
            }

            public void Upsert<T>(PropertyChangedMessage<T> message)
            {
                if (ShouldSuppressChanges)
                {
                    return;
                }

                var propertyName = message.PropertyName;
                if (string.IsNullOrEmpty(propertyName))
                {
                    return;
                }

                if (_changes.TryGetValue(propertyName!, out var changes) && changes is Change<T> actualChanges)
                {
                    // update, the initial value is kept, so that reverting to it is not considered a change
                    actualChanges.NewValue = message.NewValue;
                    actualChanges.IsActualChange = !AreEqual(actualChanges.InitialValue, message.NewValue);
                }
                else
                {
                    // initial value
                    _changes[propertyName!] = new Change<T>(message.OldValue, message.NewValue, !AreEqual(message.OldValue, message.NewValue));
                }
            }

            public void Clear()
            {
                _changes.Clear();
            }

            public IDisposable SkipChanges()
            {
                return new IgnoreLocalChanges(this);
            }

            private static bool AreEqual<T>(T oldValue, T newValue)
            {
                return EqualityComparer<T>.Default.Equals(oldValue, newValue);
            }

            public void Increment()
            {
                Interlocked.Increment(ref _suppressedState);
            }

            public void Decrement()
            {
                Interlocked.Decrement(ref _suppressedState);
            }

            private sealed class IgnoreLocalChanges : IDisposable
            {
                private readonly Changes _instance;

                private int _disposed;

                public IgnoreLocalChanges(Changes instance)
                {
                    _instance = instance;
                    _instance.Increment();
                }

                public void Dispose()
                {
                    if (Interlocked.Exchange(ref _disposed, 1) == 0)
                    {
                        _instance.Decrement();
                    }
                }
            }
        }

        private abstract class Change : IChange
        {
            public bool IsActualChange { get; set; }
        }

        private sealed class Change<T> : Change, IChange<T>
        {
            /// <summary>
            /// Gets the value that the property had before the change.
            /// </summary>
            public T InitialValue { get; }

            /// <summary>
            /// Gets the value that the property has after the change.
            /// </summary>
            public T NewValue { get; set; }

            public Change(T oldValue, T newValue, bool isActualChange)
            {
                InitialValue = oldValue;
                NewValue = newValue;
                IsActualChange = isActualChange;
            }
        }
    }
}
