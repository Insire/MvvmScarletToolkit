using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.Diagnostics;

namespace MvvmScarletToolkit.Wpf.Samples
{
    [DebuggerDisplay("Position {X};{Y}")]
    public sealed class Position : ObservableObject
    {
        private int _x;
        public int X
        {
            get { return _x; }
            set { SetProperty(ref _x, value); }
        }

        private int _y;
        public int Y
        {
            get { return _y; }
            set { SetProperty(ref _y, value); }
        }

        public Position()
        {
        }

        public Position(int x, int y)
            : this()
        {
            X = x;
            Y = y;
        }
    }
}
