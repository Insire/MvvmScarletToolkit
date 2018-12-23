using System;

namespace MvvmScarletToolkit.Commands
{
    public interface ICommandManager
    {
        void InvalidateRequerySuggested();

        event EventHandler RequerySuggested;
    }
}
