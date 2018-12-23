using System;

namespace MvvmScarletToolkit.Abstractions
{
    public interface ICommandManager
    {
        void InvalidateRequerySuggested();

        event EventHandler RequerySuggested;
    }
}
