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

            _messenger.Subscribe<PositionUpdatedMessage>((message) =>
            {
                Messages.PushBack(message);
            });

            _messenger.Subscribe<SnakeSegmentCreatedMessage>((message) =>
            {
                Messages.PushBack(message);
            });
        }
    }
}
