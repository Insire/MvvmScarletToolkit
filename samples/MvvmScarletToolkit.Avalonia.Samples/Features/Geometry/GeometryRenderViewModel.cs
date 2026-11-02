using Avalonia;
using Avalonia.Media;
using MvvmScarletToolkit.Observables;
using System.Globalization;

namespace MvvmScarletToolkit.Avalonia.Samples.Features.Geometry
{
    public sealed class GeometryRenderViewModel : BusinessViewModelListBase<GeometryContainer>
    {
        private static readonly Dictionary<char, global::Avalonia.Media.Geometry> Geometries = new();

        public GeometryRenderViewModel(IScarletCommandBuilder commandBuilder)
            : base(commandBuilder)
        {
        }

        private static global::Avalonia.Media.Geometry GetGeometry(char character)
        {
            if (Geometries.TryGetValue(character, out var geometry))
            {
                return geometry;
            }

            var text = new FormattedText(character.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 24, Brushes.Black);

            return Geometries[character] = text.BuildGeometry(new Point(9, 9)) ?? new StreamGeometry();
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            // one dispatcher call for all items, AddRange would dispatch each item on its own and in no particular order.
            // Geometries can only be used on the UI thread they were created on, unlike the frozen ones in WPF
            await Dispatcher.Invoke(() =>
            {
                for (var i = 0; i < 5000; i++)
                {
                    AddUnchecked(new GeometryContainer(GetGeometry((char)((i % (97 - 32)) + 32)), CommandBuilder.Messenger));
                }

                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(nameof(HasItems));
            }, token).ConfigureAwait(false);
        }
    }
}
