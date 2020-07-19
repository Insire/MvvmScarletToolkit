using MvvmScarletToolkit.Abstractions;
using MvvmScarletToolkit.Observables;
using System;

namespace MvvmScarletToolkit.Samples
{
    public sealed class SnakeLogViewModel : ObservableObject
    {
        private DateTime _nextUpdate;

        private ObservableCircularBuffer<ScarletMessageBase> _messages;
        public ObservableCircularBuffer<ScarletMessageBase> Messages
        {
            get { return _messages; }
            private set { SetValue(ref _messages, value); }
        }

        private int _messagesPerSecond;
        public int MessagesPerSecond
        {
            get { return _messagesPerSecond; }
            private set { SetValue(ref _messagesPerSecond, value); }
        }

        public SnakeLogViewModel(IScarletMessenger messenger)
        {
            Messages = new ObservableCircularBuffer<ScarletMessageBase>(100);

            if (messenger == null)
            {
                throw new ArgumentNullException(nameof(messenger));
            }

            messenger.Subscribe<PositionUpdatedMessage>(MessageSubscription);
            messenger.Subscribe<SnakeSegmentCreatedMessage>(MessageSubscription);
            messenger.Subscribe<SnakeDirectionChanged>(MessageSubscription);

            _nextUpdate = DateTime.UtcNow.AddSeconds(1);
        }

        private void MessageSubscription(SnakeSegmentCreatedMessage message)
        {
            PushMessage(message);
        }

        private void MessageSubscription(PositionUpdatedMessage message)
        {
            PushMessage(message);
        }

        private void MessageSubscription(SnakeDirectionChanged message)
        {
            PushMessage(message);
        }

        private void PushMessage(ScarletMessageBase message)
        {
            Messages.Push(message);
#pragma warning disable INPC003 // Notify when property changes.
            _messagesPerSecond++;

            if (_nextUpdate > DateTime.UtcNow)
            {
                return;
            }

            OnPropertyChanged(nameof(MessagesPerSecond));
            _messagesPerSecond = 0;
#pragma warning restore INPC003 // Notify when property changes.

            _nextUpdate = DateTime.UtcNow.AddSeconds(1);
        }
    }
}
