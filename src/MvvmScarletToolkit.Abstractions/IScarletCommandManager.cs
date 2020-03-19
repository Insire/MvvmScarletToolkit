using System;

namespace MvvmScarletToolkit
{
    public interface IScarletCommandManager
    {
        void InvalidateRequerySuggested();

        event EventHandler RequerySuggested;
    }
}
