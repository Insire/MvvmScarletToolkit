using CommunityToolkit.Mvvm.ComponentModel;

namespace MvvmScarletToolkit.Avalonia.Samples.Features.Geometry
{
    public sealed class GeometryRow : ObservableObject
    {
        public IReadOnlyList<global::Avalonia.Media.Geometry> Items { get; }

        public GeometryRow(IReadOnlyList<global::Avalonia.Media.Geometry> items)
        {
            Items = items;
        }
    }
}
