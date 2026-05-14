using CommunityToolkit.Mvvm.ComponentModel;

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
        public T? Default { get; }

        /// <summary>
        /// new value
        /// </summary>
        public virtual T? Current
        {
            get;
            set
            {
                if (!SetProperty(ref field, value))
                {
                    return;
                }

                if (!EqualityComparer<T>.Default.Equals(field!, Default!))
                {
                    HasChanged = true;
                }

                OnPropertyChanged(nameof(CurrentOrDefault));
            }
        }

        public T? CurrentOrDefault => HasChanged ? Current : Default;

        public VersionViewModel(in T? defaultValue)
        {
            Default = defaultValue;
        }

        public VersionViewModel(in T? defaultValue, in T? current)
        {
            Default = defaultValue;
            Current = current;
        }
    }
}
