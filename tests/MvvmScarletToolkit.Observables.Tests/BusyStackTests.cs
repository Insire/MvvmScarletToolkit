namespace MvvmScarletToolkit.Observables.Tests
{
    public sealed class BusyStackTests : TraceTestBase
    {
        [Fact]
        public void Ctor_DoesThrow_For_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new BusyStack(null!));
        }

        [Fact]
        public void Push_And_Pull_Do_Notify_On_State_Changes_Only()
        {
            var notifications = new List<bool>();
            var sut = new BusyStack(notifications.Add);

            sut.Push();
            sut.Push();
            sut.Pull();
            sut.Pull();

            Assert.Equal([true, false], notifications);
        }

        [Fact]
        public void GetToken_Does_Push_And_Pull()
        {
            var notifications = new List<bool>();
            var sut = new BusyStack(notifications.Add);

            using (sut.GetToken())
            {
                Assert.Equal([true], notifications);
            }

            Assert.Equal([true, false], notifications);
        }

        [Fact]
        public void Token_Disposed_Twice_Does_Pull_Once()
        {
            var notifications = new List<bool>();
            var sut = new BusyStack(notifications.Add);

            var outer = sut.GetToken();
            var inner = sut.GetToken();
            inner.Dispose();
            inner.Dispose();

            // the outer token still holds the stack busy
            Assert.Equal([true], notifications);

            outer.Dispose();

            Assert.Equal([true, false], notifications);
        }

        [Fact]
        public async Task Push_And_Pull_Are_Threadsafe()
        {
            var busy = 0;
            var notifications = 0;
            var sut = new BusyStack(isBusy =>
            {
                Interlocked.Increment(ref notifications);
                Interlocked.Exchange(ref busy, isBusy ? 1 : 0);
            });

            // keep the stack busy for the whole run, so concurrent tokens must never flip the state
            using (sut.GetToken())
            {
                await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(() =>
                {
                    for (var i = 0; i < 10_000; i++)
                    {
                        using (sut.GetToken())
                        {
                        }
                    }
                }, TestContext.Current.CancellationToken)));

                Assert.Equal(1, busy);
                Assert.Equal(1, notifications);
            }

            Assert.Equal(0, busy);
            Assert.Equal(2, notifications);
        }
    }
}
