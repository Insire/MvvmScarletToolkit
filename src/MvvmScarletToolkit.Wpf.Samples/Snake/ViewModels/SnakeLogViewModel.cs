using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Messaging;
using MvvmScarletToolkit.Observables;
using System;

namespace MvvmScarletToolkit.Wpf.Samples
{
    public sealed class SnakeLogViewModel : ObservableRecipient
    {
        private DateTime _nextUpdate;

        private ObservableCircularBuffer<ScarletMessageBase> _messages;
        public ObservableCircularBuffer<ScarletMessageBase> Messages
        {
            get { return _messages; }
            private set { SetProperty(ref _messages, value); }
        }

        private int _messagesPerSecond;
        public int MessagesPerSecond
        {
            get { return _messagesPerSecond; }
            private set { SetProperty(ref _messagesPerSecond, value); }
        }

        public SnakeLogViewModel(IMessenger messenger)
            : base(messenger)
        {
            Messages = new ObservableCircularBuffer<ScarletMessageBase>(100);

            Messenger.Register<PositionUpdatedMessage>(this, (r, m) => MessageSubscription(m));
            Messenger.Register<SnakeSegmentCreatedMessage>(this, (r, m) => MessageSubscription(m));
            Messenger.Register<SnakeDirectionChanged>(this, (r, m) => MessageSubscription(m));

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
