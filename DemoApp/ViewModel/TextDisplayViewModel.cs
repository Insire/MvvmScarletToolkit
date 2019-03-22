using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace DemoApp
{
    public sealed class TextDisplayViewModel : ViewModelListBase<GeometryContainer>
    {
        private readonly Typeface _typeface;
        private readonly NumberSubstitution _numberSubstitution;

        private readonly IEnumerable<GeometryContainer> _geomtries;

        public TextDisplayViewModel(CommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            _typeface = new Typeface("Tahoma");
            _numberSubstitution = new NumberSubstitution();

            _geomtries = Enumerable.Range(0, 5000)
                                    .Select(c => (c % (97 - 32)) + 32)
                                    .Select(c => (char)c)
                                    .Select(c => new GeometryContainer(BuildGeometry(new string(c, 1), _typeface, _numberSubstitution)))
                                    .ToArray();
        }

        protected override async Task Load(CancellationToken token)
        {
            await AddRange(_geomtries).ConfigureAwait(false);
        }

        protected override async Task Refresh(CancellationToken token)
        {
            await AddRange(_geomtries).ConfigureAwait(false);
        }

        private static Geometry BuildGeometry(string charachters, Typeface typeface, NumberSubstitution numberSubstitution)
        {
            var result = new FormattedText(charachters, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 24, Brushes.Black, numberSubstitution, TextFormattingMode.Display)
                            .BuildGeometry(new Point(9, 9));

            result.Freeze();

            return result;
        }
    }

    public sealed class GeometryContainer : ViewModelContainer<Geometry>
    {
        public GeometryContainer(Geometry value)
            : base(value)
        {
        }
    }
}
