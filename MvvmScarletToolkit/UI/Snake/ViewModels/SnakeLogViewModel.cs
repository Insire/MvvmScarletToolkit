using System;

namespace MvvmScarletToolkit
{
    public sealed class SnakeLogViewModel : ObservableObject
    {
        private readonly IMessenger _messenger;
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

        public SnakeLogViewModel(IMessenger messenger)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            Messages = new ObservableCircularBuffer<ScarletMessageBase>(100);

            _messenger.Subscribe<PositionUpdatedMessage>(PositionUpdatedMessageSubscription);
            _messenger.Subscribe<SnakeSegmentCreatedMessage>(SnakeSegmentCreatedMessageSubscription);

            _nextUpdate = DateTime.UtcNow.AddSeconds(1);
        }

        private void SnakeSegmentCreatedMessageSubscription(SnakeSegmentCreatedMessage message)
        {
            PushMessage(message);
        }

        private void PositionUpdatedMessageSubscription(PositionUpdatedMessage message)
        {
            PushMessage(message);
        }

        private void PushMessage(ScarletMessageBase message)
        {
            Messages.PushFront(message);
            _messagesPerSecond++;

            if (_nextUpdate > DateTime.UtcNow)
                return;

            OnPropertyChanged(nameof(MessagesPerSecond));
            _messagesPerSecond = 0;

            _nextUpdate = DateTime.UtcNow.AddSeconds(1);
        }
    }
}
