namespace MvvmScarletToolkit.UnitTests.Mocks
{
    internal sealed class Positionable : IPositionable
    {
        public Position CurrentPosition { get; set; }
        public Size Size { get; set; }
    }
}
