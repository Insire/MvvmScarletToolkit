using MvvmScarletToolkit;

namespace MvvmScarletToolkit.Samples
{
    public sealed class SnakeDirectionChanged : GenericScarletMessage<Direction>
    {
        public SnakeDirectionChanged(object sender, Direction content)
            : base(sender, content)
        {
        }
    }
}
