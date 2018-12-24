using MvvmScarletToolkit.Abstractions;
using System;
using System.Windows.Input;

namespace MvvmScarletToolkit.Implementations
{
    /// <summary>
    /// simple wrapper around <see cref="System.Windows.Input.CommandManager"/>
    /// </summary>
    public sealed class ScarletCommandManager : ICommandManager
    {
        public static ICommandManager Default { get; } = new ScarletCommandManager();

        public event EventHandler RequerySuggested
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void InvalidateRequerySuggested()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
