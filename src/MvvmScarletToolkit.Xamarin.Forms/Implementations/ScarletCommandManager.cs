using System;

namespace MvvmScarletToolkit
{
    public sealed class ScarletCommandManager : IScarletCommandManager
    {
        private static readonly Lazy<ScarletCommandManager> _default = new Lazy<ScarletCommandManager>(() => new ScarletCommandManager());

        public static IScarletCommandManager Default { get; } = _default.Value;

        public event EventHandler? RequerySuggested;

        public void InvalidateRequerySuggested()
        {
            RequerySuggested?.Invoke(this, EventArgs.Empty);
        }
    }
}
