namespace b1.Main
{
    public interface IMessageChannel
    {
        public Task PublishAsync<TMessage>(TMessage e);
        public void Subscribe<TMessage>(Func<TMessage,Task> handler);
    }
}