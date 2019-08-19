using MvvmScarletToolkit.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit
{
    internal sealed class ViewModelAdapter<TViewModel> : IVirtualizedListViewModel
        where TViewModel : class, IVirtualizationViewModel
    {
        private readonly IBusinessViewModelListBase<TViewModel> _viewModelListBase;

        public ViewModelAdapter(IBusinessViewModelListBase<TViewModel> viewModelListBase)
        {
            _viewModelListBase = viewModelListBase ?? throw new ArgumentNullException(nameof(viewModelListBase));
        }

        public async Task Deflate(object item, CancellationToken token)
        {
            if (item is TViewModel viewModel && _viewModelListBase.Items.Contains(viewModel))
            {
                await viewModel.Unload(token).ConfigureAwait(false);
            }
        }

        public async Task Extend(IEnumerable<object> items, CancellationToken token)
        {
            await items
                    .Where(p => p is TViewModel)
                    .Cast<TViewModel>()
                    .ForEachAsync(p => p.Load(token));
        }
    }
}
