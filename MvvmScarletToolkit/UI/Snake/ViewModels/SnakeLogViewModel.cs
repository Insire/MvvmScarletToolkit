using System;

namespace MvvmScarletToolkit
{
    public sealed class SnakeLogViewModel : ObservableObject
    {
        private readonly IMessenger _messenger;

        private ObservableCircularBuffer<ScarletMessageBase> _messages;
        public ObservableCircularBuffer<ScarletMessageBase> Messages
        {
            get { return _messages; }
            private set { SetValue(ref _messages, value); }
        }

        public SnakeLogViewModel(IMessenger messenger)
        {
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            Messages = new ObservableCircularBuffer<ScarletMessageBase>(100);

            _messenger.Subscribe<PositionUpdatedMessage>(PositionUpdatedMessageSubscription);
            _messenger.Subscribe<SnakeSegmentCreatedMessage>(SnakeSegmentCreatedMessageSubscription);
        }

        private void SnakeSegmentCreatedMessageSubscription(SnakeSegmentCreatedMessage message)
        {
            Messages.PushFront(message);
        }

        private void PositionUpdatedMessageSubscription(PositionUpdatedMessage message)
        {
            Messages.PushFront(message);
        }
    }
}
