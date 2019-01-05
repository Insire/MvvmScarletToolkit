using System;

namespace MvvmScarletToolkit.Abstractions
{
    public interface IScarletCommandManager
    {
        void InvalidateRequerySuggested();

        event EventHandler RequerySuggested;
    }
}
