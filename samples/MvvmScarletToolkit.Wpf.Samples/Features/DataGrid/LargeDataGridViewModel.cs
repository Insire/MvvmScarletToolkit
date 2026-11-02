using CommunityToolkit.Mvvm.ComponentModel;
using MvvmScarletToolkit.Observables;
using System.Windows.Data;

namespace MvvmScarletToolkit.Wpf.Samples.Features.DataGrid
{
    /// <summary>
    /// A grid with a million rows that are all in memory, to show what the DataGrid extensions do at that size.
    /// The rows are a plain list: an observable collection would raise a change for each of them.
    /// </summary>
    public sealed partial class LargeDataGridViewModel : BusinessViewModelBase
    {
        public const int RowCount = 1_000_000;

        private static readonly DateTime _start = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static readonly Dictionary<string, Func<LargeRowViewModel, object?>> _readers = new()
        {
            [nameof(LargeRowViewModel.Id)] = p => p.Id,
            [nameof(LargeRowViewModel.Reference)] = p => p.Reference,
            [nameof(LargeRowViewModel.Name)] = p => p.Name,
            [nameof(LargeRowViewModel.Description)] = p => p.Description,
            [nameof(LargeRowViewModel.Category)] = p => p.Category,
            [nameof(LargeRowViewModel.IsActive)] = p => p.IsActive,
            [nameof(LargeRowViewModel.IsVerified)] = p => p.IsVerified,
            [nameof(LargeRowViewModel.Quantity)] = p => p.Quantity,
            [nameof(LargeRowViewModel.Views)] = p => p.Views,
            [nameof(LargeRowViewModel.Score)] = p => p.Score,
            [nameof(LargeRowViewModel.Price)] = p => p.Price,
            [nameof(LargeRowViewModel.CreatedOn)] = p => p.CreatedOn,
            [nameof(LargeRowViewModel.ModifiedOn)] = p => p.ModifiedOn,
            [nameof(LargeRowViewModel.ReleaseDate)] = p => p.ReleaseDate,
            [nameof(LargeRowViewModel.StartTime)] = p => p.StartTime,
            [nameof(LargeRowViewModel.Duration)] = p => p.Duration,
        };

        [ObservableProperty]
        public partial IReadOnlyList<LargeRowViewModel>? Items { get; private set; }

        public GroupingViewModel Groups { get; }

        public DataGridFilterViewModel ColumnFilters { get; }

        public CsvExporter<LargeRowViewModel> CsvExporter { get; }

        /// <summary>
        /// Stored by the tracker configured in <see cref="App"/>
        /// </summary>
        [ObservableProperty]
        public partial DataGridLayout? Layout { get; set; }

        public LargeDataGridViewModel(IScarletCommandBuilder commandBuilder, ExportFileDialog exportFileDialog)
            : base(commandBuilder)
        {
            Groups = GroupingViewModel.Create<LargeRowViewModel>(
                commandBuilder,
                () => (ListCollectionView)CollectionViewSource.GetDefaultView(Items),
                new GroupingViewModelOptions { ExcludeToolkitProperties = true });

            ColumnFilters = DataGridFilterViewModel.Create<LargeRowViewModel>(commandBuilder);
            ColumnFilters.GetFilter(nameof(LargeRowViewModel.Name))!.ValidationRules.Add(new MinimumLengthValidationRule(2));

            CsvExporter = new CsvExporter<LargeRowViewModel>(commandBuilder, exportFileDialog, _readers);
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            var items = await Task.Run(() => CreateRows(token), token).ConfigureAwait(false);

            await Dispatcher.Invoke(() => Items = items, token).ConfigureAwait(false);
        }

        protected override Task UnloadInternal(CancellationToken token)
        {
            return Dispatcher.Invoke(() => Items = null, token);
        }

        private static LargeRowViewModel[] CreateRows(CancellationToken token)
        {
            // a fixed seed, so that the rows are the same on every start
            var random = new Random(42);
            var categories = Enum.GetValues<RowCategory>();
            var bytes = new byte[16];
            var rows = new LargeRowViewModel[RowCount];

            for (var i = 0; i < rows.Length; i++)
            {
                if (i % 65536 == 0)
                {
                    token.ThrowIfCancellationRequested();
                }

                random.NextBytes(bytes);

                var createdOn = _start.AddMinutes(random.Next(0, 3_000_000));

                rows[i] = new LargeRowViewModel
                {
                    Id = i,
                    Reference = new Guid(bytes),
                    Name = $"Item {i}",
                    Description = i % 5 == 0 ? null : $"Description of item {i}",
                    Category = categories[random.Next(categories.Length)],
                    IsActive = random.Next(2) == 0,
                    IsVerified = (i % 3) switch { 0 => true, 1 => false, _ => null },
                    Quantity = i % 7 == 0 ? null : random.Next(0, 1000),
                    Views = random.NextInt64(0, 10_000_000_000),
                    Score = Math.Round(random.NextDouble() * 100, 2),
                    Price = random.Next(1, 1_000_000) / 100m,
                    CreatedOn = createdOn,
                    ModifiedOn = new DateTimeOffset(createdOn.AddHours(random.Next(0, 5000))),
                    ReleaseDate = DateOnly.FromDateTime(createdOn.AddDays(random.Next(0, 365))),
                    StartTime = new TimeOnly(random.Next(0, 24), random.Next(0, 60)),
                    Duration = TimeSpan.FromSeconds(random.Next(0, 100_000)),
                };
            }

            return rows;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Groups.Dispose();
                ColumnFilters.Dispose();
                CsvExporter.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
