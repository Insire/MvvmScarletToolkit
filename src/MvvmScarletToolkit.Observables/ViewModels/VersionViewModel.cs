using System.Collections.Generic;

namespace MvvmScarletToolkit.Observables
{
    /// <summary>
    /// Helper viewmodel for tracking changes
    /// </summary>
    /// <typeparam name="T">literally any c# object</typeparam>
    public class VersionViewModel<T> : ObservableObject
    {
        protected bool HasChanged { get; private set; }

        /// <summary>
        /// original value
        /// </summary>
        public T Default { get; }

        private T _current;
        /// <summary>
        /// new value
        /// </summary>
        public virtual T Current
        {
            get { return _current; }
            set
            {
                if (SetValue(ref _current, value))
                {
                    if (!EqualityComparer<T>.Default.Equals(_current, Default))
                    {
                        HasChanged = true;
                    }

                    OnPropertyChanged(nameof(CurrentOrDefault));
                }
            }
        }

        public T CurrentOrDefault => HasChanged ? Current : Default;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public VersionViewModel(T defaultValue)
        {
            Default = defaultValue;
        }

        public VersionViewModel(T defaultValue, T current)
        {
            Default = defaultValue;
            Current = current;
        }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}
