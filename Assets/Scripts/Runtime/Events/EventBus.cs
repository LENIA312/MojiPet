using System;
using System.Collections.Generic;

namespace Mojipet.Events
{
    public sealed class EventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var eventType = typeof(TEvent);
            if (!_handlers.TryGetValue(eventType, out var handlerList))
            {
                handlerList = new List<Delegate>();
                _handlers.Add(eventType, handlerList);
            }

            handlerList.Add(handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var eventType = typeof(TEvent);
            if (_handlers.TryGetValue(eventType, out var handlerList))
            {
                handlerList.Remove(handler);
            }
        }

        public void Publish<TEvent>(TEvent eventData)
        {
            var eventType = typeof(TEvent);
            if (!_handlers.TryGetValue(eventType, out var handlerList))
            {
                return;
            }

            for (var i = 0; i < handlerList.Count; i++)
            {
                var handler = (Action<TEvent>)handlerList[i];
                handler.Invoke(eventData);
            }
        }
    }
}
