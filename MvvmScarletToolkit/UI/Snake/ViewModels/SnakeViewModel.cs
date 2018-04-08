using System.Collections.ObjectModel;

namespace MvvmScarletToolkit
{
    public sealed class SnakeViewModel : ObservableObject
    {
        private SnakeOption _selectedOption;
        public SnakeOption SelectedOption
        {
            get { return _selectedOption; }
            set { SetValue(ref _selectedOption, value); }
        }

        private ObservableCollection<SnakeOption> _options;
        public ObservableCollection<SnakeOption> Options
        {
            get { return _options; }
            private set { SetValue(ref _options, value); }
        }

        private KeyMapViewModel _keyMapViewModel;
        public KeyMapViewModel KeyMapViewModel
        {
            get { return _keyMapViewModel; }
            private set { SetValue(ref _keyMapViewModel, value); }
        }

        public SnakeViewModel()
        {
            KeyMapViewModel = new KeyMapViewModel();
            Options = new ObservableCollection<SnakeOption>()
            {
                SnakeOption.Easy(),
                SnakeOption.Normal(),
                SnakeOption.Hard(),
                new SnakeOption(),
            };

            SelectedOption = Options[1];
        }

        // [Keyboard support]
        // filter invalid keys, or provide valid subset from enum collection

        // [screen management]
        // - add fail screen/state
        // - update game screen
        // - add debug screen (virtualized log?)

        // [logging]
        // - provide a visual log
        // - virtualize the log

        // [customization]
        // - load images displaying board pieces
        // - provide custom images
    }
}
