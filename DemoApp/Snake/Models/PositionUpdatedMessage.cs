using MvvmScarletToolkit;

namespace DemoApp
{
    public sealed class PositionUpdatedMessage : ScarletMessageBase
    {
        public Position From { get; }
        public Position To { get; }

        public PositionUpdatedMessage(object sender, Position from, Position to)
            : base(sender)
        {
            From = from;
            To = to;
        }
    }
}
