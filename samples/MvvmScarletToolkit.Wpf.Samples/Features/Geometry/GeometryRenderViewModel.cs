using MvvmScarletToolkit.Observables;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace MvvmScarletToolkit.Wpf.Samples.Features.Geometry
{
    public sealed class GeometryRenderViewModel : BusinessViewModelListBase<GeometryContainer>
    {
        private readonly Typeface _typeface;
        private readonly NumberSubstitution _numberSubstitution;

        // built on first load and off the UI thread, instead of while the application starts
        private readonly Lazy<GeometryContainer[]> _geomtries;

        public GeometryRenderViewModel(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            _typeface = new Typeface("Tahoma");
            _numberSubstitution = new NumberSubstitution();

            _geomtries = new Lazy<GeometryContainer[]>(() => Enumerable.Range(0, 5000)
                                    .Select(c => (c % (97 - 32)) + 32)
                                    .Select(c => (char)c)
                                    .Select(c => new GeometryContainer(BuildGeometry(new string(c, 1), _typeface, _numberSubstitution), commandBuilder.Messenger))
                                    .ToArray());
        }

        private static System.Windows.Media.Geometry BuildGeometry(string charachters, Typeface typeface, NumberSubstitution numberSubstitution)
        {
            var result = new FormattedText(charachters, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 24, Brushes.Black, numberSubstitution, TextFormattingMode.Display, 90)
                            .BuildGeometry(new Point(9, 9));

            result.Freeze();

            return result;
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            var geometries = await Task.Run(() => _geomtries.Value, token).ConfigureAwait(false);

            // one dispatcher call for all items, AddRange would dispatch each item on its own and in no particular order
            await Dispatcher.Invoke(() =>
            {
                foreach (var geometry in geometries)
                {
                    AddUnchecked(geometry);
                }

                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(nameof(HasItems));
            }, token).ConfigureAwait(false);
        }
    }
}
