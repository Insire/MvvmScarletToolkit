using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Wpf.Samples.Features.DataGrid
{
    public sealed class DataGridDataProvider : IPagedDataProvider<DataGridRowViewModel>
    {
        private readonly IScarletCommandBuilder _scarletCommandBuilder;
        private readonly List<DataGridRowViewModel> _cache;

        public DataGridDataProvider(IScarletCommandBuilder scarletCommandBuilder, int pageCount, int pageSize)
        {
            _cache = new List<DataGridRowViewModel>(pageCount * pageSize);
            _scarletCommandBuilder = scarletCommandBuilder ?? throw new ArgumentNullException(nameof(scarletCommandBuilder));

            var page = 1;
            for (var i = 0; i < pageCount * pageSize; i++)
            {
                if (i % pageSize == 0)
                {
                    page++;
                }
                _cache.Add(new DataGridRowViewModel(_scarletCommandBuilder, page)
                {
                    Id = i,
                    CreatedOn = DateTime.Now,
                    Name = Guid.NewGuid().ToString(),
                    Color = $"#cc{i * 2:X2}{i * 3:X2}",
                });
            }
        }

        public Task<ICollection<DataGridRowViewModel>> Get(int index, int count, CancellationToken token)
        {
            return Task.FromResult<ICollection<DataGridRowViewModel>>(_cache.AsQueryable().TryPage(index, count).ToList());
        }
    }
}
