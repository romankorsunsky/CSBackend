using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.Tracing;

namespace b1.Main
{
    public class DefaultMessageChannel : IMessageChannel
    {
        private ConcurrentDictionary<Type,List<Delegate>> _map { get; init; }
        public DefaultMessageChannel()
        {
            _map = new ConcurrentDictionary<Type, List<Delegate>>();
        }
        public void Publish<TMessage>(TMessage e)
        {
            if (_map.TryGetValue(typeof(TMessage), out var handlers))
            {
                foreach (var handler in handlers)
                {
                    handler.DynamicInvoke();
                }
            }
        }

        public void Subscribe<TMessage>(Action<TMessage> handler)
        {
            if(!_map.ContainsKey(typeof(TMessage))){
                _map[typeof(TMessage)] = new List<Delegate>();
            }
            _map[typeof(TMessage)].Add(handler);
        }
    }
}