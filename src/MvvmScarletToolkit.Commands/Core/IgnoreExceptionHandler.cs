using System;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Commands
{
    public sealed class IgnoreExceptionHandler : IScarletExceptionHandler
    {
        private static readonly Lazy<IScarletExceptionHandler> _default = new Lazy<IScarletExceptionHandler>(() => new IgnoreExceptionHandler());

        public static IScarletExceptionHandler Default => _default.Value;

        public Task Handle(Exception ex)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine(ex.ToString());
#endif

            return Task.CompletedTask;
        }
    }
}
