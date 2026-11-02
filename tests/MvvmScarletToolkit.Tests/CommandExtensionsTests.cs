using System.Windows.Input;

namespace MvvmScarletToolkit.Tests
{
    public sealed class CommandExtensionsTests : TraceTestBase
    {
        [Fact]
        public void TryExecute_Should_Execute_When_The_Command_Allows_It()
        {
            var command = Substitute.For<ICommand>();
            command.CanExecute("parameter").Returns(true);

            command.TryExecute("parameter");

            command.Received(1).Execute("parameter");
        }

        [Fact]
        public void TryExecute_Should_Not_Execute_When_The_Command_Denies_It()
        {
            var command = Substitute.For<ICommand>();
            command.CanExecute("parameter").Returns(false);

            command.TryExecute("parameter");

            command.DidNotReceive().Execute(Arg.Any<object?>());
        }
    }
}
