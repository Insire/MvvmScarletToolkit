using System;
using System.ComponentModel;

namespace MvvmScarletToolkit
{
    public interface IChangeTracker : IDisposable
    {
        /// <summary>
        /// Check if any tracked instance was changed
        /// </summary>
        bool HasChanges();

        /// <summary>
        /// Check if the provided instance is being tracked and was changed
        /// </summary>
        bool HasChanges<T>(T instance)
            where T : class, INotifyPropertyChanged;

        /// <summary>
        /// Track whether <paramref name="instance"/> raised <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// </summary>
        /// <param name="instance">The instance to track</param>
        /// <exception cref="ObjectDisposedException"></exception>
        void Track<T>(T instance)
            where T : class, INotifyPropertyChanged;

        /// <summary>
        /// Stops tracking all instances
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        void StopTracking();

        /// <summary>
        /// Stops tracking <paramref name="instance"/>
        /// </summary>
        /// <param name="instance"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        void StopTracking<T>(T instance)
            where T : class, INotifyPropertyChanged;

        /// <summary>
        /// Discards any tracked changes
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        void ClearChanges();

        /// <summary>
        /// Discards any tracked changes for <paramref name="instance"/>
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        void ClearChanges<T>(T instance)
            where T : class, INotifyPropertyChanged;

        /// <summary>
        /// Discards any new change notification, while the returned subscription is active
        /// </summary>
        /// <returns>the subscription</returns>
        /// <exception cref="ObjectDisposedException"></exception>
        IDisposable SuppressAllChanges();

        /// <summary>
        /// Discards any new change notification for <paramref name="instance"/>, while the returned subscription is active
        /// </summary>
        /// <returns>the subscription</returns>
        /// <exception cref="ObjectDisposedException"></exception>
        IDisposable SuppressChanges<T>(T instance)
            where T : class, INotifyPropertyChanged;
    }
}
