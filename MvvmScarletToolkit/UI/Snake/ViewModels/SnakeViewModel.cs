using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

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

        private SnakeLogViewModel _snakeLogViewModel;
        public SnakeLogViewModel SnakeLogViewModel
        {
            get { return _snakeLogViewModel; }
            private set { SetValue(ref _snakeLogViewModel, value); }
        }

        public SnakeViewModel(SnakeLogViewModel snakeLogViewModel)
        {
            SnakeLogViewModel = snakeLogViewModel ?? throw new ArgumentNullException(nameof(snakeLogViewModel));

            KeyMapViewModel = new KeyMapViewModel();
            Options = new ObservableCollection<SnakeOption>()
            {
                SnakeOption.Easy(),
                SnakeOption.Normal(),
                SnakeOption.Hard(),
                new SnakeOption(),
            };

            if (Debugger.IsAttached)
                SelectedOption = Options[3];
            else
                SelectedOption = Options[1];
        }

        // [screen management]
        // - fix gamestate change not beeing recognized, when running the game more than once
        // - fix processing of input not being fast enough, so that you can fail the game unexpectedly

        // [optimization, clean code, logging]
        // switch from property checks to event pipeline

        // [Keyboard support]
        // filter invalid keys, or provide valid subset from enum collection

        // [customization]
        // - load images displaying board pieces
        // - provide custom images
    }
}
