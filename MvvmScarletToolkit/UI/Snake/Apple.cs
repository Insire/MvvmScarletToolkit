namespace MvvmScarletToolkit
{
    public sealed class Apple : IPositionable
    {
        public Position CurrentPosition { get; }

        public Apple(int x, int y)
        {
            CurrentPosition = new Position
            {
                X = x,
                Y = y,
            };
        }
    }
}
