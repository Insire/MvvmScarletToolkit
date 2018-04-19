﻿using System;

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

            _messenger.Subscribe<PositionUpdatedMessage>(MessageSubscription);
            _messenger.Subscribe<SnakeSegmentCreatedMessage>(MessageSubscription);
            _messenger.Subscribe<SnakeDirectionChanged>(MessageSubscription);

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
            _messagesPerSecond++;

            if (_nextUpdate > DateTime.UtcNow)
                return;

            OnPropertyChanged(nameof(MessagesPerSecond));
            _messagesPerSecond = 0;

            _nextUpdate = DateTime.UtcNow.AddSeconds(1);
        }
    }
}