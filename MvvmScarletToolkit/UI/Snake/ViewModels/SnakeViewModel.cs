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

        // [core functionality]
        // bind buttons to "change direction"

        // [core functionality]
        // snake cant travel over its own body
        // add area check to snake head checking its segements for intersections

        // [Keyboard support]
        // set up keyboard shortcuts (keymap)
        // wasd, spacebar
        // arrL,arrR,arrU,arrD, insert, ctrl
        // bind keyboard shortcuts to "change direction"

        // [screen management]
        // add general screen management
        // add fail screen/state
        // add options screen
        // update game screen
        // add debug screen (virtualized log?)

        // [customization]
        // load images displaying board pieces
        // provide custom images
    }
}
