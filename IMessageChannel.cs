namespace b1.Main
{
    public interface IMessageChannel
    {
        public Task PublishEvent<TMessage>(TMessage e);
        public Task SubscribeToEvent<TMessage>(Func<TMessage,Task> handler);
    }
}