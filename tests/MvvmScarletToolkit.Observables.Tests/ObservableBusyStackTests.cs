namespace MvvmScarletToolkit.Observables.Tests
{
    public sealed class ObservableBusyStackTests : TraceTestBase
    {
        [Fact]
        public void Ctor_DoesThrow_For_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new ObservableBusyStack(null!));
        }

        [Fact]
        public void Push_And_Pull_Do_Notify_Owner_On_State_Changes_Only()
        {
            var notifications = new List<bool>();
            using var sut = new ObservableBusyStack(notifications.Add);

            sut.Push();
            sut.Push();
            sut.Pull();
            sut.Pull();

            Assert.Equal([true, false], notifications);
        }

        [Fact]
        public void Push_And_Pull_Do_Notify_Subscribers()
        {
            using var sut = new ObservableBusyStack(_ => { });
            var observer = Substitute.For<IObserver<bool>>();

            using (sut.Subscribe(observer))
            {
                using (sut.GetToken())
                {
                }
            }

            Received.InOrder(() =>
            {
                observer.OnNext(true);
                observer.OnCompleted();
            });
        }

        [Fact]
        public void Unsubscribed_Observers_Are_Not_Notified()
        {
            using var sut = new ObservableBusyStack(_ => { });
            var observer = Substitute.For<IObserver<bool>>();

            sut.Subscribe(observer).Dispose();

            using (sut.GetToken())
            {
            }

            observer.DidNotReceiveWithAnyArgs().OnNext(default);
            observer.DidNotReceive().OnCompleted();
        }

        [Fact]
        public void Dispose_Does_Throw_On_Further_Usage()
        {
            var sut = new ObservableBusyStack(_ => { });
            sut.Dispose();

            Assert.Throws<ObjectDisposedException>(() => sut.Push());
            Assert.Throws<ObjectDisposedException>(() => sut.GetToken());
            Assert.Throws<ObjectDisposedException>(() => sut.Subscribe(Substitute.For<IObserver<bool>>()));
        }

        [Fact]
        public void Dispose_Twice_DoesNotThrow()
        {
            var sut = new ObservableBusyStack(_ => { });
            sut.Dispose();
            sut.Dispose();
        }

        [Fact]
        public void Token_Released_After_Dispose_DoesNotThrow()
        {
            var notifications = new List<bool>();
            var sut = new ObservableBusyStack(notifications.Add);

            var token = sut.GetToken();
            sut.Dispose();
            token.Dispose();

            // the owner is not notified anymore, after the stack has been disposed
            Assert.Equal([true], notifications);
        }

        [Fact]
        public void ToString_Does_Contain_State()
        {
            using var sut = new ObservableBusyStack(_ => { });

            Assert.Contains("HasItems: False", sut.ToString());

            using (sut.GetToken())
            {
                Assert.Contains("HasItems: True", sut.ToString());
            }
        }
    }
}
