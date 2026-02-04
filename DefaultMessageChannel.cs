using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.Tracing;
using b1.Messages;
using ScottPlot.Plottables;

namespace b1.Main
{
    public class DefaultMessageChannel : IMessageChannel
    {
        private static DefaultMessageChannel _instance;
        private ConcurrentDictionary<Type, List<Action<object>>> _eventHandlers { get; init; }

        static DefaultMessageChannel(){
            _instance = new DefaultMessageChannel();
        }
        private DefaultMessageChannel()
        {
            _eventHandlers = new ConcurrentDictionary<Type, List<Action<object>>>();
        }
        public static DefaultMessageChannel GetInstance()
        {
            return _instance;
        }
        public Task PublishEvent<TMessage>(TMessage e)
        {
            if (_eventHandlers.TryGetValue(typeof(TMessage), out var handlers))
            {
                lock (handlers)
                {
                    foreach (var handler in handlers)
                    {
                        handler(e);
                    }
                }
            }
            return Task.CompletedTask;
        }

        public Task SubscribeToEvent<TMessage>(Func<TMessage, Task> handler)
        {
            var handlers = _eventHandlers.GetOrAdd(typeof(TMessage), _ => new List<Action<object>>());
            lock (handlers)
            {
                handlers.Add(message => handler((TMessage)message));
            }
            return Task.CompletedTask;
        }
    }
}