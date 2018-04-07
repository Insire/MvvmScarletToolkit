﻿using System.Diagnostics;

namespace MvvmScarletToolkit
{
    [DebuggerDisplay("Position {X};{Y}")]
    public sealed class Position : ObservableObject
    {
        private int _x;
        public int X
        {
            get { return _x; }
            set { SetValue(ref _x, value); }
        }

        private int _y;
        public int Y
        {
            get { return _y; }
            set { SetValue(ref _y, value); }
        }


        public Position()
        {
        }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
