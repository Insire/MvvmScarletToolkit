using System;
using System.Linq;
using System.Windows.Input;
using MvvmScarletToolkit;
using System.Collections.Generic;

namespace DemoApp
{
    public class ParentViewModel : ObservableObject
    {
        private PropertyObserver<DemoItems> _demoItemsObserver;
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

        public ParentViewModel()
        {
            DemoItems = new DemoItems();
            LogItems = new LogItems();
            // TODO
            //_demoItemsObserver = new PropertyObserver<DemoItems>(DemoItems);
            //_demoItemsObserver.RegisterHandler(item=>item.SelectedItem, p=>LogItems.Items.Where(logItem=>logItem.Message == p.))

            AddLinkedCommand = new RelayCommand(AddLinked, CanAddLink);
            AddRangeCommand = new RelayCommand(AddRange, CanAddRange);
        }

        public void AddLinked()
        {
            var timeStamp = DateTime.UtcNow.ToLongTimeString();
            DemoItems.Items.Add(new DemoItem(timeStamp));
            LogItems.Items.Add(new LogItem(timeStamp));
        }

        private bool CanAddLink()
        {
            return LogItems.Items != null
                && DemoItems.Items != null;
        }

        public void AddRange()
        {
            var items = new List<string>();
            for (var i = 0; i < 5; i++)
            {
                items.Add(Guid.NewGuid().ToString());
            }

            LogItems.AddRange(items.Select(p => new LogItem(p)));
            DemoItems.AddRange(items.Select(p => new DemoItem(p)));
        }

        private bool CanAddRange()
        {
            return LogItems.Items != null
                && DemoItems.Items != null;
        }
    }
}
