using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// A cell of a <see cref="DataGridSummaryBar"/>
    /// </summary>
    public sealed partial class DataGridSummaryCell : ObservableObject
    {
        [ObservableProperty]
        public partial double Width { get; set; }

        [ObservableProperty]
        public partial string? Text { get; set; }

        /// <summary>
        /// The tooltip of this cell, saying what <see cref="Text"/> means, like "Sum of Price", and how to change it
        /// </summary>
        [ObservableProperty]
        public partial string? Description { get; set; }

        /// <summary>
        /// Identifies the latest calculation of this cell, older ones don't get to set <see cref="Text"/>
        /// </summary>
        internal int Version { get; set; }
    }
}
