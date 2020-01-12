using MvvmScarletToolkit.Observables;
using System.Windows.Media;

namespace DemoApp
{
    public sealed class GeometryContainer : ViewModelContainer<Geometry>
    {
        public GeometryContainer(Geometry value)
            : base(value)
        {
        }
    }
}
