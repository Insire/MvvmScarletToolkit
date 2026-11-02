using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Samples.Features.DataGrid
{
    public enum RowCategory
    {
        Hardware,
        Software,
        Service,
        Support,
        Other,
    }

    /// <summary>
    /// A row with the most common data types. There are a million of these, so this derives from <see cref="ObservableObject"/> instead of ViewModelBase, which allocates a lot more per instance.
    /// </summary>
    public sealed class LargeRowViewModel : ObservableObject
    {
        public required int Id { get; init; }

        public required Guid Reference { get; init; }

        public required string Name { get; init; }

        public string? Description { get; init; }

        public required RowCategory Category { get; init; }

        public required bool IsActive { get; init; }

        public bool? IsVerified { get; init; }

        public int? Quantity { get; init; }

        public required long Views { get; init; }

        public required double Score { get; init; }

        public required decimal Price { get; init; }

        public required DateTime CreatedOn { get; init; }

        public required DateTimeOffset ModifiedOn { get; init; }

        public required DateOnly ReleaseDate { get; init; }

        public required TimeOnly StartTime { get; init; }

        public required TimeSpan Duration { get; init; }
    }
}
