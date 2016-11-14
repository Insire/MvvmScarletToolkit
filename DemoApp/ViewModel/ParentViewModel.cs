using System;
using System.Linq;
using System.Windows.Input;
using MvvmScarletToolkit;

namespace DemoApp
{
    public class ParentViewModel : ObservableObject
    {
        private PropertyObserver<DemoItems> _demoItemsObserver;
        public ICommand AddLinkedCommand { get; private set; }

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
    }
}
