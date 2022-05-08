using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MvvmScarletToolkit.Incubator
{
    /// <summary>
    /// A ChangeTracker based on <see cref="INotifyPropertyChanged"/>
    /// </summary>
    public sealed class NotifyProperyChangedTracker : IDisposable
    {
        private readonly Dictionary<INotifyPropertyChanged, ChangeState> _changes;

        private bool disposedValue;

        private bool ShouldSuppressChanges { get; set; }

        /// <summary>
        /// Check if any tracked instance was changed
        /// </summary>
        public bool HasChanges()
        {
            return _changes.Values.Any(p => p.IsChanged);
        }

        /// <summary>
        /// Check if the provided instance is being tracked and was changed
        /// </summary>
        public bool HasChanges(INotifyPropertyChanged @this)
        {
            if (_changes.TryGetValue(@this, out var changes))
            {
                return changes.IsChanged;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Track <paramref name="this"/> instance
        /// </summary>
        /// <param name="this">The instance to track</param>
        /// <exception cref="ObjectDisposedException"></exception>
        public void Track(INotifyPropertyChanged @this)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(nameof(NotifyProperyChangedTracker));
            }

            @this.PropertyChanged -= OnPropertyChanged;
            @this.PropertyChanged += OnPropertyChanged;
        }

        /// <summary>
        /// Stops and removes all previously tracked instances
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        public void StopTracking()
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(nameof(NotifyProperyChangedTracker));
            }
        }

        /// <summary>
        /// Stops and removes the previously tracked instance
        /// </summary>
        /// <param name="this"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        public void StopTracking(INotifyPropertyChanged @this)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(nameof(NotifyProperyChangedTracker));
            }

            @this.PropertyChanged -= OnPropertyChanged;
        }

        /// <summary>
        /// Discards any tracked changes
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        public void ClearChanges()
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(nameof(NotifyProperyChangedTracker));
            }
        }

        /// <summary>
        /// Discards any tracked changes for the provided instance
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        public void ClearChanges(INotifyPropertyChanged @this)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(nameof(NotifyProperyChangedTracker));
            }

            if (_changes.TryGetValue(@this, out var changes))
            {
                _changes.Remove(@this);
            }
        }

        /// <summary>
        /// Discards any new change notification, while the returned subscription is active
        /// </summary>
        /// <returns>the subscription</returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public IDisposable SuppressAllChanges()
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(nameof(NotifyProperyChangedTracker));
            }

            return new SuppressAllChangesSubscription(this);
        }

        /// <summary>
        /// Discards any new change notification for <paramref name="this"/>, while the returned subscription is active
        /// </summary>
        /// <returns>the subscription</returns>
        /// <exception cref="ObjectDisposedException"></exception>
        public IDisposable SuppressChanges(INotifyPropertyChanged @this)
        {
            if (disposedValue)
            {
                throw new ObjectDisposedException(nameof(NotifyProperyChangedTracker));
            }

            if (_changes.TryGetValue(@this, out var changes))
            {
                return new SuppressChangesSubscription(changes);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(@this), "instance is not being tracked");
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ShouldSuppressChanges)
            {
                return;
            }

            if (sender is INotifyPropertyChanged observable)
            {
                if (_changes.TryGetValue(observable, out var changes))
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
            ShouldSuppressChanges = true;

            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var change in _changes)
                    {
                        change.Key.PropertyChanged -= OnPropertyChanged;
                    }

                    _changes.Clear();
                }

                disposedValue = true;
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
            private readonly HashSet<PropertyChangedEventArgs> _changes;
            public bool IsChanged => _changes.Count > 0;
            public bool ShouldSuppressChanges { get; set; }

            public ChangeState()
            {
                _changes = new HashSet<PropertyChangedEventArgs>();
            }

            public void Add(PropertyChangedEventArgs args)
            {
                if (ShouldSuppressChanges)
                {
                    return;
                }

                _changes.Add(args);
            }
        }

        private sealed class SuppressChangesSubscription : IDisposable
        {
            private readonly ChangeState _changeState;

            public SuppressChangesSubscription(ChangeState changeState)
            {
                _changeState = changeState;
                _changeState.ShouldSuppressChanges = true;
            }

            public void Dispose()
            {
                _changeState.ShouldSuppressChanges = false;
            }
        }

        private sealed class SuppressAllChangesSubscription : IDisposable
        {
            private readonly NotifyProperyChangedTracker _tracker;

            public SuppressAllChangesSubscription(NotifyProperyChangedTracker tracker)
            {
                _tracker = tracker;
                _tracker.ShouldSuppressChanges = true;
            }

            public void Dispose()
            {
                _tracker.ShouldSuppressChanges = false;
            }
        }
    }
}
