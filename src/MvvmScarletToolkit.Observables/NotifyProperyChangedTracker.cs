using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// A ChangeTracker based on <see cref="INotifyPropertyChanged"/>
    /// </summary>
    /// <remarks>
    /// This class is not threadsafe.
    /// </remarks>
    public sealed class NotifyProperyChangedTracker : IChangeTracker
    {
        private readonly Dictionary<INotifyPropertyChanged, ChangeState> _trackedInstances;

        private bool _disposedValue;
        private bool _shouldSuppressChanges;

        public NotifyProperyChangedTracker()
        {
            _trackedInstances = new Dictionary<INotifyPropertyChanged, ChangeState>();
        }

        /// <inheritdoc/>
        public bool HasChanges()
        {
            return _trackedInstances.Values.Any(p => p.IsChanged);
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
                throw new ObjectDisposedException(nameof(NotifyProperyChangedTracker));
            }

            if (_trackedInstances.TryGetValue(instance, out var changes))
            {
                return changes.IsChanged;
            }
            else
            {
                return false;
            }
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
                throw new ObjectDisposedException(nameof(NotifyProperyChangedTracker));
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
        public void Track<T>(T instance)
            where T : class, INotifyPropertyChanged
        {
            if (instance is null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(NotifyProperyChangedTracker));
            }

            instance.PropertyChanged -= OnPropertyChanged;
            instance.PropertyChanged += OnPropertyChanged;

            _trackedInstances[instance] = new ChangeState();
        }

        /// <inheritdoc/>
        public void StopAllTracking()
        {
            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(NotifyProperyChangedTracker));
            }

            using (SuppressAllChanges())
            {
                foreach (var instance in _trackedInstances.Keys)
                {
                    instance.PropertyChanged -= OnPropertyChanged;
                }
            }
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
                throw new ObjectDisposedException(nameof(NotifyProperyChangedTracker));
            }

            if (_trackedInstances.TryGetValue(instance, out var changeState))
            {
                instance.PropertyChanged -= OnPropertyChanged;
            }
        }

        /// <inheritdoc/>
        public void ClearAllChanges()
        {
            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(NotifyProperyChangedTracker));
            }

            foreach (var state in _trackedInstances.Values.ToArray())
            {
                state.Clear();
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
                throw new ObjectDisposedException(nameof(NotifyProperyChangedTracker));
            }

            if (_trackedInstances.TryGetValue(instance, out var state))
            {
                state.Clear();
            }
        }

        /// <inheritdoc/>
        public IDisposable SuppressAllChanges()
        {
            if (_disposedValue)
            {
                throw new ObjectDisposedException(nameof(NotifyProperyChangedTracker));
            }

            return new SuppressAllChangesSubscription(this);
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
                throw new ObjectDisposedException(nameof(NotifyProperyChangedTracker));
            }

            if (_trackedInstances.TryGetValue(instance, out var changes))
            {
                return new SuppressChangesSubscription(changes);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(instance), "instance is not being tracked");
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_shouldSuppressChanges)
            {
                return;
            }

            if (sender is INotifyPropertyChanged observable)
            {
                if (_trackedInstances.TryGetValue(observable, out var changes))
                {
                    if (changes.ShouldSuppressChanges)
                    {
                        return;
                    }

                    changes.Add(e);
                }
            }
        }

        private void Dispose(bool disposing)
        {
            _shouldSuppressChanges = true;

            if (!_disposedValue)
            {
                if (disposing)
                {
                    foreach (var instance in _trackedInstances.Keys.ToArray())
                    {
                        instance.PropertyChanged -= OnPropertyChanged;
                    }

                    _trackedInstances.Clear();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private sealed class ChangeState
        {
            private readonly HashSet<string> _changes;

            private int _suppressedState = 0;

            public int Count => _changes.Count;
            public bool IsChanged => _changes.Count > 0;
            public bool ShouldSuppressChanges => _suppressedState != 0;

            public ChangeState()
            {
                _changes = new HashSet<string>();
            }

            public void Add(PropertyChangedEventArgs args)
            {
                if (ShouldSuppressChanges)
                {
                    return;
                }

                _changes.Add(args.PropertyName);
            }

            public void Increment()
            {
                Interlocked.Increment(ref _suppressedState);
            }

            public void Decrement()
            {
                Interlocked.Decrement(ref _suppressedState);
            }

            public void Clear()
            {
                _changes.Clear();
            }
        }

        private sealed class SuppressChangesSubscription : IDisposable
        {
            private readonly ChangeState _changeState;

            public SuppressChangesSubscription(ChangeState changeState)
            {
                _changeState = changeState;
                _changeState.Increment();
            }

            public void Dispose()
            {
                _changeState.Decrement();
            }
        }

        private sealed class SuppressAllChangesSubscription : IDisposable
        {
            private readonly NotifyProperyChangedTracker _tracker;

            public SuppressAllChangesSubscription(NotifyProperyChangedTracker tracker)
            {
                _tracker = tracker;
                _tracker._shouldSuppressChanges = true;
            }

            public void Dispose()
            {
                _tracker._shouldSuppressChanges = false;
            }
        }
    }
}
