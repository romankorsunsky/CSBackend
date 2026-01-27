namespace b1.Main
{
    public interface IMessageChannel
    {
        public Task PublishEvent<TMessage>(TMessage e);
        public void SubscribeToEvent<TMessage>(Action<TMessage> handler);

        public Task ExecuteCommandAsync<ICommand>(ICommand command);

        public Task AdoptCommandAsync<ICommand>(Func<ICommand, Task> handler);
    }
}