using CommunityToolkit.Mvvm.Messaging;

namespace MvvmScarletToolkit.Avalonia.Samples.Features.Geometry
{
    public sealed class GeometryContainer : ViewModelContainer<global::Avalonia.Media.Geometry>
    {
        public GeometryContainer(global::Avalonia.Media.Geometry value, IMessenger messenger)
            : base(value, messenger)
        {
        }
    }
}
