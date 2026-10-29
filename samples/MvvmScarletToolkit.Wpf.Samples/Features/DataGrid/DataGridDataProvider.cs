namespace MvvmScarletToolkit.Wpf.Samples.Features.DataGrid
{
    public sealed class DataGridDataProvider : IPagedDataProvider<DataGridRowViewModel>
    {
        private readonly IScarletCommandBuilder _scarletCommandBuilder;
        private readonly int _pageSize;
        private readonly int _totalCount;

        // rows are created when they are requested for the first time and handed out again on later requests,
        // so that the paged list sees the same instances, when it loads a page again
        private readonly Dictionary<int, DataGridRowViewModel> _cache;
        private readonly Lock _cacheLock;

        public DataGridDataProvider(IScarletCommandBuilder scarletCommandBuilder, int pageCount, int pageSize)
        {
            _scarletCommandBuilder = scarletCommandBuilder ?? throw new ArgumentNullException(nameof(scarletCommandBuilder));
            _pageSize = pageSize;
            _totalCount = pageCount * pageSize;
            _cache = new Dictionary<int, DataGridRowViewModel>();
            _cacheLock = new Lock();
        }

        public Task<ICollection<DataGridRowViewModel>> Get(int index, int count, CancellationToken token)
        {
            var start = Math.Max(0, index);
            var end = Math.Min(start + Math.Max(0, count), _totalCount);
            var result = new List<DataGridRowViewModel>(Math.Max(0, end - start));

            lock (_cacheLock)
            {
                for (var i = start; i < end; i++)
                {
                    if (!_cache.TryGetValue(i, out var row))
                    {
                        row = Create(i);
                        _cache.Add(i, row);
                    }

                    result.Add(row);
                }
            }

            return Task.FromResult<ICollection<DataGridRowViewModel>>(result);
        }

        private DataGridRowViewModel Create(int i)
        {
            return new DataGridRowViewModel(_scarletCommandBuilder, (i / _pageSize) + 1)
            {
                Id = i,
                CreatedOn = DateTime.Now,
                Name = Guid.NewGuid().ToString(),
                // each channel has to stay within two hex digits, for a valid #RRGGBB color
                Color = $"#cc{(i * 2) & 0xFF:X2}{(i * 3) & 0xFF:X2}",
            };
        }
    }
}
