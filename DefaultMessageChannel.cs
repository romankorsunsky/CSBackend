using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.Tracing;
using b1.Messages;

namespace b1.Main
{
    public class DefaultMessageChannel : IMessageChannel
    {
        private ConcurrentDictionary<Type,List<Func<object,Task>>> _map { get; init; }
        public DefaultMessageChannel()
        {
            _map = new ConcurrentDictionary<Type, List<Func<object,Task>>>();
        }
        public async Task PublishAsync<TMessage>(TMessage e)
        {
            if (_map.TryGetValue(typeof(TMessage), out var handlers))
            {
                var taskList = new List<Task>();
                lock (handlers)
                {
                    foreach (var handler in handlers)
                    {
                        taskList.Add(handler(e));
                    }
                }
                await Task.WhenAll(taskList);
            }
        }

        public void Subscribe<TMessage>(Func<TMessage,Task> handler)
        {
            var handlers = _map.GetOrAdd(typeof(TMessage), _ => new List<Func<object,Task>>());
            lock(handlers){
                handlers.Add(message => handler((TMessage)message));
            }
        }
    }
}