using CommunityToolkit.Mvvm.Messaging;

namespace MvvmScarletToolkit.Observables.Tests
{
    public sealed class ViewModelBaseActivationTests : TraceTestBase
    {
        // internal instead of private, the RegisterAll source generator of CommunityToolkit.Mvvm has to reach recipients
        internal sealed class TestMessage
        {
        }

        internal sealed class RecipientViewModel : ViewModelBase, IRecipient<TestMessage>
        {
            public int ReceivedCount { get; private set; }

            public RecipientViewModel(IScarletCommandBuilder commandBuilder)
                : base(commandBuilder)
            {
            }

            public void Receive(TestMessage message)
            {
                ReceivedCount++;
            }

            public void RegisterExplicitly()
            {
                Messenger.Register<RecipientViewModel, TestMessage>(this, static (r, m) => r.Receive(m));
            }
        }

        private static IScarletCommandBuilder CreateCommandBuilder(IMessenger messenger)
        {
            var commandBuilder = Substitute.For<IScarletCommandBuilder>();
            commandBuilder.Messenger.Returns(messenger);

            return commandBuilder;
        }

        [Fact]
        public void Activation_Does_Not_Register_Recipient_Implementations()
        {
            var messenger = new StrongReferenceMessenger();
            using var sut = new RecipientViewModel(CreateCommandBuilder(messenger));

            sut.IsActive = true;
            messenger.Send(new TestMessage());

            Assert.Multiple(() =>
            {
                Assert.True(sut.IsActive);
                Assert.False(messenger.IsRegistered<TestMessage>(sut));
                Assert.Equal(0, sut.ReceivedCount);
            });
        }

        [Fact]
        public void Deactivation_Does_Unregister_Explicit_Handlers()
        {
            var messenger = new StrongReferenceMessenger();
            using var sut = new RecipientViewModel(CreateCommandBuilder(messenger));

            sut.IsActive = true;
            sut.RegisterExplicitly();
            messenger.Send(new TestMessage());

            sut.IsActive = false;
            messenger.Send(new TestMessage());

            Assert.Multiple(() =>
            {
                Assert.False(messenger.IsRegistered<TestMessage>(sut));
                Assert.Equal(1, sut.ReceivedCount);
            });
        }
    }
}
