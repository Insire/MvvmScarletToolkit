namespace MvvmScarletToolkit
{
    public sealed class SnakeHead : SnakeBase
    {
        public SnakeHead(SnakeOptions options)
            : base(options)
        {
            CurrentPosition = options.GetStartingPosition();
        }
    }
}
