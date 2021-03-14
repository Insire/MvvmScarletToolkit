using MvvmScarletToolkit.Observables;
using System.Windows.Media;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class GeometryContainer : ViewModelContainer<Geometry>
    {
        public GeometryContainer(Geometry value)
            : base(value)
        {
        }
    }
}
