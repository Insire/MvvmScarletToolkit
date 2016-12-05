using MvvmScarletToolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DemoApp
{
    public class ParentViewModel : ObservableObject, IIsBusy
    {
        public ICommand AddLinkedCommand { get; private set; }
        public ICommand AddRangeCommand { get; private set; }

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

        private BusyStack _busyStack;
        public BusyStack BusyStack
        {
            get { return _busyStack; }
            private set { SetValue(ref _busyStack, value); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            private set { SetValue(ref _isBusy, value); }
        }

        public ParentViewModel()
        {
            BusyStack = new BusyStack();
            BusyStack.OnChanged = (hasItems) =>
              {
                  IsBusy = hasItems;
              };

            DemoItems = new DemoItems();
            LogItems = new LogItems();

            AddLinkedCommand = new AsyncRelayCommand(AddLinked, CanAddLink);
            AddRangeCommand = new AsyncRelayCommand(AddRange, CanAddRange);
        }

        public async Task AddLinked()
        {
            using (BusyStack.GetToken())
            {
                var result = await Task.Run(() =>
                {
                    return DateTime.UtcNow.ToLongTimeString();
                });

                DemoItems.Items.Add(new DemoItem(result));
                LogItems.Items.Add(new LogItem(result));
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
                });

                LogItems.AddRange(result.Select(p => new LogItem(p)));
                DemoItems.AddRange(result.Select(p => new DemoItem(p)));
            }
        }

        private bool CanAddRange()
        {
            return LogItems.Items != null
                && DemoItems.Items != null;
        }
    }
}
