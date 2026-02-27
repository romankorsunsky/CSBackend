namespace b1.Infrastructure
{
    public interface IMessageChannel
    {
        public Task PublishEvent<TMessage>(TMessage e);
        public Task SubscribeToEvent<TMessage>(Func<TMessage,Task> handler);
    }
}