using MvvmScarletToolkit;
using MvvmScarletToolkit.Observables;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace DemoApp
{
    public class DataContextSchenanigansViewModel : BusinessViewModelListBase<AsyncDemoItem>
    {
        public FilterViewModel<AsyncDemoItem> Filter { get; }

        private string _filterText;
        public string FilterText
        {
            get { return _filterText; }
            set { SetValue(ref _filterText, value); }
        }

        public DataContextSchenanigansViewModel(ICommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            Filter = new FilterViewModel<AsyncDemoItem>(commandBuilder, _items, new Func<AsyncDemoItem, bool>((p) => FilterText.IndexOf(p.DisplayName, StringComparison.CurrentCultureIgnoreCase) >= 0), new AsyncDemoItemComparer());
        }

        protected override async Task RefreshInternal(CancellationToken token)
        {
            for (var i = 0; i < 10; i++)
            {
                var item = new AsyncDemoItem(CommandBuilder)
                {
                    DisplayName = "Test " + i,
                };

                await Add(item).ConfigureAwait(false);
            }
        }
    }

    public class AsyncDemoItemComparer : Comparer<AsyncDemoItem>
    {
        public override int Compare([AllowNull] AsyncDemoItem x, [AllowNull] AsyncDemoItem y)
        {
            if (x.DisplayName.CompareTo(y.DisplayName) != 0)
            {
                return x.DisplayName.CompareTo(y.DisplayName);
            }
            else
            {
                return 0;
            }
        }
    }
}
