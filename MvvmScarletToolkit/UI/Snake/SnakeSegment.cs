namespace MvvmScarletToolkit
{
    public sealed class SnakeSegment : SnakeBase
    {
        public SnakeSegment(SnakeOptions options, SnakeHead head)
            : base(options)
        {
            CurrentPosition = head.CurrentPosition;
        }
    }
}
