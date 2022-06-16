namespace MvvmScarletToolkit
{
    public interface IChange
    {
        bool IsActualChange { get; }
    }

    public interface IChange<T> : IChange
    {
        /// <summary>
        /// Gets the value that the property had before the change.
        /// </summary>
        T InitialValue { get; }

        /// <summary>
        /// Gets the value that the property has after the change.
        /// </summary>
        T NewValue { get; }
    }
}
