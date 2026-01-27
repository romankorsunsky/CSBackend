using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.Tracing;
using b1.Messages;

namespace b1.Main
{
    public class DefaultMessageChannel : IMessageChannel
    {
        private ConcurrentDictionary<Type, List<Action<object>>> _eventHandlers { get; init; }
        private ConcurrentDictionary<Type, Func<object,Task>> _commandExecutors { get; init; }
        public DefaultMessageChannel()
        {
            _eventHandlers = new ConcurrentDictionary<Type, List<Action<object>>>();
            _commandExecutors = new ConcurrentDictionary<Type, Func<object, Task>>();
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

        public void SubscribeToEvent<TMessage>(Action<TMessage> handler)
        {
            var handlers = _eventHandlers.GetOrAdd(typeof(TMessage), _ => new List<Action<object>>());
            lock (handlers)
            {
                handlers.Add(message => handler((TMessage)message));
            }
        }

        public Task ExecuteCommandAsync<ICommand>(ICommand command)
        {
            throw new NotImplementedException();
        }

        public Task AdoptCommandAsync<ICommand>(Func<ICommand, Task> handler)
        {
            throw new NotImplementedException();
        }

        /*
        *should add Unsubscribe method
        */
    }
}