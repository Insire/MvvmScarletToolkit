using System.Diagnostics;

namespace MvvmScarletToolkit
{
    [DebuggerDisplay("Head {CurrentPosition.X};{CurrentPosition.Y}")]
    public sealed class SnakeHead : SnakeBase
    {
        public SnakeHead(SnakeOption options, ILogger log)
            : base(options, log)
        {
            CurrentPosition = options.GetStartingPosition();
        }
    }
}
