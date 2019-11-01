using MvvmScarletToolkit.Abstractions;
using System;
using System.Windows.Input;

namespace MvvmScarletToolkit
{
    /// <summary>
    /// simple wrapper around <see cref="System.Windows.Input.CommandManager"/>
    /// </summary>
    public sealed class ScarletCommandManager : IScarletCommandManager
    {
        private static readonly Lazy<ScarletCommandManager> _default = new Lazy<ScarletCommandManager>(() => new ScarletCommandManager());

        public static IScarletCommandManager Default { get; } = _default.Value;

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
