namespace MvvmScarletToolkit
{
    public sealed class SnakeSegmentCreatedMessage : GenericScarletMessage<SnakeSegment>
    {
        public SnakeSegmentCreatedMessage(object sender, SnakeSegment content)
            : base(sender, content)
        {
        }
    }
}
