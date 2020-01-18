using MvvmScarletToolkit.Abstractions;
using NUnit.Framework;
using System;
using System.Threading;

namespace MvvmScarletToolkit.Tests
{
    public sealed class MessengerTests
    {
        [Test]
        public void Ctor_DoesNotThrow()
        {
            new ScarletMessenger(new ScarletMessageProxy());
        }

        [Test]
        public void Subscribe_ValidDeliverAction_DoesNotThrow()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());

            messenger.Subscribe<TestMessage>(new Action<TestMessage>(Utils.FakeDeliveryAction<TestMessage>));
        }

        [Test]
        public void SubScribe_ValidDeliveryAction_ReturnsRegistrationObject()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());

            var output = messenger.Subscribe<TestMessage>(new Action<TestMessage>(Utils.FakeDeliveryAction<TestMessage>));

            Assert.IsInstanceOf<SubscriptionToken>(output);
        }

        [Test]
        public void Subscribe_ValidDeliverActionWIthStrongReferences_DoesNotThrow()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());

            messenger.Subscribe<TestMessage>(new Action<TestMessage>(Utils.FakeDeliveryAction<TestMessage>), true);
        }

        [Test]
        public void Subscribe_ValidDeliveryActionAndFilter_DoesNotThrow()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());

            messenger.Subscribe<TestMessage>(new Action<TestMessage>(Utils.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(Utils.FakeMessageFilter<TestMessage>));
        }

        [Test]
        public void Subscribe_NullDeliveryAction_Throws()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());

            Assert.Throws<ArgumentNullException>(() => messenger.Subscribe<TestMessage>(null, new Func<TestMessage, bool>(Utils.FakeMessageFilter<TestMessage>)));
        }

        [Test]
        public void Subscribe_NullFilter_Throws()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());

            Assert.Throws<ArgumentNullException>(() => messenger.Subscribe<TestMessage>(new Action<TestMessage>(Utils.FakeDeliveryAction<TestMessage>), null, new TestProxy()));
        }

        [Test]
        public void Subscribe_NullProxy_Throws()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());

            Assert.Throws<ArgumentNullException>(() => messenger.Subscribe<TestMessage>(new Action<TestMessage>(Utils.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(Utils.FakeMessageFilter<TestMessage>), null));
        }

        [Test]
        public void Unsubscribe_NullSubscriptionObject_Throws()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());

            Assert.Throws<ArgumentNullException>(() => messenger.Unsubscribe(null));
        }

        [Test]
        public void Unsubscribe_PreviousSubscription_DoesNotThrow()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            var subscription = messenger.Subscribe<TestMessage>(new Action<TestMessage>(Utils.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(Utils.FakeMessageFilter<TestMessage>));

            messenger.Unsubscribe(subscription);
        }

        [Test]
        public void Subscribe_PreviousSubscription_ReturnsDifferentSubscriptionObject()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            var sub1 = messenger.Subscribe<TestMessage>(new Action<TestMessage>(Utils.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(Utils.FakeMessageFilter<TestMessage>));
            var sub2 = messenger.Subscribe<TestMessage>(new Action<TestMessage>(Utils.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(Utils.FakeMessageFilter<TestMessage>));

            Assert.IsFalse(object.ReferenceEquals(sub1, sub2));
        }

        [Test]
        public void Subscribe_CustomProxyNoFilter_DoesNotThrow()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            var proxy = new TestProxy();

            messenger.Subscribe<TestMessage>(new Action<TestMessage>(Utils.FakeDeliveryAction<TestMessage>), proxy);
        }

        [Test]
        public void Subscribe_CustomProxyWithFilter_DoesNotThrow()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            var proxy = new TestProxy();

            messenger.Subscribe<TestMessage>(new Action<TestMessage>(Utils.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(Utils.FakeMessageFilter<TestMessage>), proxy);
        }

        [Test]
        public void Subscribe_CustomProxyNoFilterStrongReference_DoesNotThrow()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            var proxy = new TestProxy();

            messenger.Subscribe<TestMessage>(new Action<TestMessage>(Utils.FakeDeliveryAction<TestMessage>), true, proxy);
        }

        [Test]
        public void Subscribe_CustomProxyFilterStrongReference_DoesNotThrow()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            var proxy = new TestProxy();

            messenger.Subscribe<TestMessage>(new Action<TestMessage>(Utils.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(Utils.FakeMessageFilter<TestMessage>), true, proxy);
        }

        [Test]
        public void Publish_CustomProxyNoFilter_UsesCorrectProxy()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            var proxy = new TestProxy();
            messenger.Subscribe<TestMessage>(new Action<TestMessage>(Utils.FakeDeliveryAction<TestMessage>), proxy);
            var message = new TestMessage(this);

            messenger.Publish<TestMessage>(message);

            Assert.AreSame(message, proxy.Message);
        }

        [Test]
        public void Publish_CustomProxyWithFilter_UsesCorrectProxy()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            var proxy = new TestProxy();
            messenger.Subscribe<TestMessage>(new Action<TestMessage>(Utils.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(Utils.FakeMessageFilter<TestMessage>), proxy);
            var message = new TestMessage(this);

            messenger.Publish<TestMessage>(message);

            Assert.AreSame(message, proxy.Message);
        }

        [Test]
        public void Publish_CustomProxyNoFilterStrongReference_UsesCorrectProxy()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            var proxy = new TestProxy();
            messenger.Subscribe<TestMessage>(new Action<TestMessage>(Utils.FakeDeliveryAction<TestMessage>), true, proxy);
            var message = new TestMessage(this);

            messenger.Publish<TestMessage>(message);

            Assert.AreSame(message, proxy.Message);
        }

        [Test]
        public void Publish_CustomProxyFilterStrongReference_UsesCorrectProxy()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            var proxy = new TestProxy();
            messenger.Subscribe<TestMessage>(new Action<TestMessage>(Utils.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(Utils.FakeMessageFilter<TestMessage>), true, proxy);
            var message = new TestMessage(this);

            messenger.Publish<TestMessage>(message);

            Assert.AreSame(message, proxy.Message);
        }

        [Test]
        public void Publish_NullMessage_Throws()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());

            Assert.Throws<ArgumentNullException>(() => messenger.Publish<TestMessage>(null));
        }

        [Test]
        public void Publish_NoSubscribers_DoesNotThrow()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());

            messenger.Publish<TestMessage>(new TestMessage(this));
        }

        [Test]
        public void Publish_Subscriber_DoesNotThrow()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            messenger.Subscribe<TestMessage>(new Action<TestMessage>(Utils.FakeDeliveryAction<TestMessage>), new Func<TestMessage, bool>(Utils.FakeMessageFilter<TestMessage>));

            messenger.Publish<TestMessage>(new TestMessage(this));
        }

        [Test]
        public void Publish_SubscribedMessageNoFilter_GetsMessage()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            var received = false;
            messenger.Subscribe<TestMessage>((m) => { received = true; });

            messenger.Publish<TestMessage>(new TestMessage(this));

            Assert.IsTrue(received);
        }

        [Test]
        public void Publish_SubscribedThenUnsubscribedMessageNoFilter_DoesNotGetMessage()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            var received = false;
            var token = messenger.Subscribe<TestMessage>((m) => { received = true; });
            messenger.Unsubscribe(token);

            messenger.Publish<TestMessage>(new TestMessage(this));

            Assert.IsFalse(received);
        }

        [Test]
        public void Publish_SubscribedMessageButFiltered_DoesNotGetMessage()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            var received = false;
            messenger.Subscribe<TestMessage>((m) => { received = true; }, (m) => false);

            messenger.Publish<TestMessage>(new TestMessage(this));

            Assert.IsFalse(received);
        }

        [Test]
        public void Publish_SubscribedMessageNoFilter_GetsActualMessage()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            IScarletMessage receivedMessage = null;
            var payload = new TestMessage(this);
            messenger.Subscribe<TestMessage>((m) => { receivedMessage = m; });

            messenger.Publish<TestMessage>(payload);

            Assert.AreSame(payload, receivedMessage);
        }

        [Test]
        public void GenericScarletMessage_String_SubscribeDoesNotThrow()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            var output = string.Empty;
            messenger.Subscribe<GenericScarletMessage<string>>((m) => { output = m.Content; });
        }

        [Test]
        public void GenericScarletMessage_String_PubishDoesNotThrow()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            messenger.Publish(new GenericScarletMessage<string>(this, "Testing"));
        }

        [Test]
        public void GenericScarletMessage_String_PubishAndSubscribeDeliversContent()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            var output = string.Empty;
            messenger.Subscribe<GenericScarletMessage<string>>((m) => { output = m.Content; });
            messenger.Publish(new GenericScarletMessage<string>(this, "Testing"));

            Assert.AreEqual("Testing", output);
        }

        [Test]
        public void Publish_SubscriptionThrowingException_DoesNotThrow()
        {
            var exceptionHandled = false;
            var messenger = new ScarletMessenger(new ExceptionHandlingProxy(HandleException));
            messenger.Subscribe<GenericScarletMessage<string>>((m) => { throw new NotImplementedException(); });

            messenger.Publish(new GenericScarletMessage<string>(this, "Testing"));

            Assert.IsTrue(exceptionHandled);

            void HandleException(Action action)
            {
                try
                {
                    action.Invoke();
                }
                catch (NotImplementedException)
                {
                    exceptionHandled = true;
                }
            }
        }

        [Test]
        public void Publish_SubscriptionThrowingException_DoesThrow()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            messenger.Subscribe<GenericScarletMessage<string>>((m) => { throw new NotImplementedException(); });

            Assert.Throws<NotImplementedException>(() => messenger.Publish(new GenericScarletMessage<string>(this, "Testing")));
        }

        [Test]
        public void PublishAsync_NoCallback_DoesNotThrow()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());

            messenger.PublishAsync(new TestMessage(this));
        }

        [Test]
        public void PublishAsync_NoCallback_PublishesMessage()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            var received = false;
            messenger.Subscribe<TestMessage>((m) => { received = true; });

            messenger.PublishAsync(new TestMessage(this));

            // Horrible wait loop!
            var waitCount = 0;
            while (!received && waitCount < 100)
            {
                Thread.Sleep(10);
                waitCount++;
            }
            Assert.IsTrue(received);
        }

        [Test]
        public void CancellableGenericScarletMessage_Publish_DoesNotThrow()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
#pragma warning disable 219
            messenger.Publish<CancellableGenericScarletMessage<string>>(new CancellableGenericScarletMessage<string>(this, "Testing", () => { var test = true; }));
#pragma warning restore 219
        }

        [Test]
        public void CancellableGenericScarletMessage_PublishWithNullAction_Throws()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            Assert.Throws<ArgumentNullException>(() => messenger.Publish<CancellableGenericScarletMessage<string>>(new CancellableGenericScarletMessage<string>(this, "Testing", null)));
        }

        [Test]
        public void CancellableGenericScarletMessage_SubscriberCancels_CancelActioned()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            var cancelled = false;
            messenger.Subscribe<CancellableGenericScarletMessage<string>>((m) => { m.Cancel(); });

            messenger.Publish<CancellableGenericScarletMessage<string>>(new CancellableGenericScarletMessage<string>(this, "Testing", () => { cancelled = true; }));

            Assert.IsTrue(cancelled);
        }

        [Test]
        public void CancellableGenericScarletMessage_SeveralSubscribersOneCancels_CancelActioned()
        {
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            var cancelled = false;
#pragma warning disable 219
            messenger.Subscribe<CancellableGenericScarletMessage<string>>((m) => { var test = 1; });
            messenger.Subscribe<CancellableGenericScarletMessage<string>>((m) => { m.Cancel(); });
            messenger.Subscribe<CancellableGenericScarletMessage<string>>((m) => { var test = 1; });
#pragma warning restore 219
            messenger.Publish<CancellableGenericScarletMessage<string>>(new CancellableGenericScarletMessage<string>(this, "Testing", () => { cancelled = true; }));

            Assert.IsTrue(cancelled);
        }

        [Test]
        public void Publish_SubscriptionOnBaseClass_HitsSubscription()
        {
            var received = false;
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            messenger.Subscribe<TestMessage>(tm => received = true);

            messenger.Publish(new DerivedMessage<string>(this) { Things = "Hello" });

            Assert.IsTrue(received);
        }

        [Test]
        public void Publish_SubscriptionOnImplementedInterface_HitsSubscription()
        {
            var received = false;
            var messenger = new ScarletMessenger(new ScarletMessageProxy());
            messenger.Subscribe<ITestMessageInterface>(tm => received = true);

            messenger.Publish(new InterfaceDerivedMessage<string>(this) { Things = "Hello" });

            Assert.IsTrue(received);
        }
    }
}
