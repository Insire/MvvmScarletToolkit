using System;

namespace MvvmScarletToolkit.Abstractions
{
    public interface IChangeTrackable
    {
        bool HasChanged { get; }

        event EventHandler Changed;
    }
}
