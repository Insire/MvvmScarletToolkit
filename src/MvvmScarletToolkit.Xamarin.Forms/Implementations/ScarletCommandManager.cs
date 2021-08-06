using System;
using Xamarin.Forms;

namespace MvvmScarletToolkit
{
    public sealed class ScarletCommandManager : IScarletCommandManager
    {
        private static readonly Lazy<ScarletCommandManager> _default = new Lazy<ScarletCommandManager>(() => new ScarletCommandManager());

        public static IScarletCommandManager Default { get; } = _default.Value;

        public event EventHandler? RequerySuggested;

        public ScarletCommandManager()
        {
        }

        public void InvalidateRequerySuggested()
        {
            var handler = RequerySuggested;
            if (handler is null)
            {
                return;
            }

            Device.BeginInvokeOnMainThread(() => handler.Invoke(this, EventArgs.Empty));
        }
    }
}
