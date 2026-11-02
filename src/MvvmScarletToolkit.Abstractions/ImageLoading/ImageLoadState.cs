namespace MvvmScarletToolkit
{
    /// <summary>
    /// State of an image that gets loaded asynchronously into an image control.
    /// </summary>
    public enum ImageLoadState
    {
        /// <summary>
        /// No source is set.
        /// </summary>
        None,

        /// <summary>
        /// A source is set, but the image has not been requested yet or the request was interrupted,
        /// e.g. because the control is not shown. Loading resumes automatically.
        /// </summary>
        Pending,

        /// <summary>
        /// The image is being fetched from a slow source, like the disk or the web. Fast in-memory hits skip this state.
        /// </summary>
        Loading,

        /// <summary>
        /// The image is shown.
        /// </summary>
        Loaded,

        /// <summary>
        /// The image could not be loaded.
        /// </summary>
        Failed,
    }
}
