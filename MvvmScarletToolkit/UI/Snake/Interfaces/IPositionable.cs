﻿namespace MvvmScarletToolkit
{
    public interface IPositionable
    {
        Position CurrentPosition { get; }
        Size Size { get; }
    }
}