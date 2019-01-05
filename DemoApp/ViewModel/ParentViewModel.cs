using MvvmScarletToolkit.Commands;
using MvvmScarletToolkit.Observables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DemoApp
{
    public class ParentViewModel : ViewModelBase
    {
        public ICommand AddLinkedCommand { get; }
        public ICommand AddRangeCommand { get; }

        private DemoItems _demoItems;
        public DemoItems DemoItems
        {
            get { return _demoItems; }
            set { SetValue(ref _demoItems, value); }
        }

        private LogItems _logItems;
        public LogItems LogItems
        {
            get { return _logItems; }
            set { SetValue(ref _logItems, value); }
        }

        public ParentViewModel(CommandBuilder commandBuilder)
            : base(commandBuilder)
        {
            DemoItems = new DemoItems(commandBuilder);
            LogItems = new LogItems(commandBuilder);

            AddLinkedCommand = CommandBuilder.Create(AddLinked, CanAddLink).Build();
            AddRangeCommand = CommandBuilder.Create(AddRange, CanAddRange).Build();
        }

        public async Task AddLinked()
        {
            using (BusyStack.GetToken())
            {
                var result = await Task.Run(() => DateTime.UtcNow.ToLongTimeString()).ConfigureAwait(false);

                await DemoItems.Add(new DemoItem(CommandBuilder, result)).ConfigureAwait(false);
                await LogItems.Add(new LogItem(result)).ConfigureAwait(false);
            }
        }

        private bool CanAddLink()
        {
            return LogItems.Items != null
                && DemoItems.Items != null;
        }

        public async Task AddRange()
        {
            using (BusyStack.GetToken())
            {
                var result = await Task.Run(() =>
                {
                    var items = new List<string>();
                    for (var i = 0; i < 5; i++)
                    {
                        items.Add(Guid.NewGuid().ToString());
                    }

                    return items;
                }).ConfigureAwait(false);

                await LogItems.AddRange(result.Select(p => new LogItem(p))).ConfigureAwait(false);
                await DemoItems.AddRange(result.Select(p => new DemoItem(CommandBuilder, p))).ConfigureAwait(false);
            }
        }

        private bool CanAddRange()
        {
            return LogItems.Items != null
                && DemoItems.Items != null;
        }

        protected override Task LoadInternal(CancellationToken token)
        {
            IsLoaded = true;
            return Task.CompletedTask;
        }

        protected override Task UnloadInternalAsync()
        {
            IsLoaded = false;
            return Task.CompletedTask;
        }

        protected override Task RefreshInternal(CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}
