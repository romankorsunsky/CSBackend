namespace b1.Main
{
    public interface IMessageChannel
    {
        public void Publish<TMessage>(TMessage e);
        public void Subscribe<TMessage>(Action<TMessage> handler);
    }
}