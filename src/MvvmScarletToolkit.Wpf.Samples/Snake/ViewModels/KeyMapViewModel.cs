using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class KeyMapViewModel : ObservableObject
    {
        private ObservableCollection<Key> _keys;
        public ObservableCollection<Key> Keys
        {
            get { return _keys; }
            private set { SetProperty(ref _keys, value); }
        }

        private Key _moveNorthKey;
        public Key MoveNorthKey
        {
            get { return _moveNorthKey; }
            set { SetProperty(ref _moveNorthKey, value); }
        }

        private Key _moveSouthKey;
        public Key MoveSouthKey
        {
            get { return _moveSouthKey; }
            set { SetProperty(ref _moveSouthKey, value); }
        }

        private Key _moveWestKey;
        public Key MoveWestKey
        {
            get { return _moveWestKey; }
            set { SetProperty(ref _moveWestKey, value); }
        }

        private Key _moveEastKey;
        public Key MoveEastKey
        {
            get { return _moveEastKey; }
            set { SetProperty(ref _moveEastKey, value); }
        }

        private Key _playKey;
        public Key PlayKey
        {
            get { return _playKey; }
            set { SetProperty(ref _playKey, value); }
        }

        public KeyMapViewModel()
        {
            Keys = new ObservableCollection<Key>(Enum.GetValues(typeof(Key)).Cast<Key>());

            MoveNorthKey = Key.W;
            MoveSouthKey = Key.S;
            MoveWestKey = Key.A;
            MoveEastKey = Key.D;

            PlayKey = Key.Space;
        }
    }
}
