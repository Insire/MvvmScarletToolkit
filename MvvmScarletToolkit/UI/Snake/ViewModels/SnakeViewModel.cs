using System.Collections.ObjectModel;

namespace MvvmScarletToolkit
{
    public sealed class SnakeViewModel : ObservableObject
    {
        private SnakeOptions _selectedOptions;
        public SnakeOptions SelectedOptions
        {
            get { return _selectedOptions; }
            set { SetValue(ref _selectedOptions, value); }
        }

        private ObservableCollection<SnakeOptions> _options;
        public ObservableCollection<SnakeOptions> Options
        {
            get { return _options; }
            private set { SetValue(ref _options, value); }
        }

        public SnakeViewModel()
        {
            Options = new ObservableCollection<SnakeOptions>()
            {
                SnakeOptions.Easy(),
                SnakeOptions.Normal(),
                SnakeOptions.Hard(),
                new SnakeOptions(),
            };

            SelectedOptions = Options[1];
        }

        // [core functionality]
        // add new movement type "change direction"
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
