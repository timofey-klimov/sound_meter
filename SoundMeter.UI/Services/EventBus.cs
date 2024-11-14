using System;
using System.Collections.Generic;
using System.Linq;

namespace SoundMeter.UI.Services
{

    internal interface IEventBusSubscriber;

    interface IEventMessage;

    internal interface IEventBus
    {
        IDisposable On<T>(IEventBusSubscriber subcriber, Action<T> message)
            where T : IEventMessage;

        void Publish<T>(T message)
            where T : IEventMessage;
    }
    internal class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<EventSubscriber>> _events = new();
        
        public IDisposable On<T>(IEventBusSubscriber subcriber, Action<T> message)
            where T : IEventMessage
        {
            var eventSubsriber = new EventSubscriber(typeof(T), subcriber, (sub) =>
            {
                var callbacks = _events[sub.MessageType];
                if (callbacks.Count > 0)
                {
                    var callback = callbacks.FirstOrDefault(x => x.Subscriber == sub.Subscriber);
                    callbacks.Remove(callback);
                }

            }, (param) => message((T)param));

            if (_events.TryGetValue(typeof(T), out var subscribers))
            {
                subscribers.Add(eventSubsriber);
            }
            else
            {
                _events.Add(typeof(T), new List<EventSubscriber> { eventSubsriber });
            }

            return eventSubsriber;
        }

        public void Publish<T>(T message)
            where T : IEventMessage
        {
            if (_events.TryGetValue(typeof(T),out var subscribers))
            {
                foreach (var subscriber in subscribers)
                {
                    subscriber?.Action(message);
                }
            }
        }


        private class EventSubscriber : IDisposable
        {
            public Type MessageType { get; }

            public IEventBusSubscriber Subscriber { get; }

            public Action<EventSubscriber> DisposeCallback { get; }

            public Action<object> Action { get; }

            public EventSubscriber(Type messageType, IEventBusSubscriber subscriber, Action<EventSubscriber> disposeCallback, Action<object> action)
            {
                MessageType = messageType;
                Subscriber = subscriber;
                DisposeCallback = disposeCallback;
                Action = action;
            }
            public void Dispose()
            {
                DisposeCallback?.Invoke(this);
            }

            public override bool Equals(object? obj)
            {
                if (obj is EventSubscriber sub)
                {
                    return this.MessageType == sub.MessageType && this.Subscriber == sub.Subscriber;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return MessageType.GetHashCode() ^ Subscriber.GetHashCode();
            }
        }
    }
}
