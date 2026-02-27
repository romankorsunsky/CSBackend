using b1.Infrastructure;
using b1.Main;
using static b1.Main.PositionCommand;

namespace b1.Repositories
{
    public interface ICommandRepository
    {
        public Task AddCommand(Command command);
        public Task<Command?> GetCommand(string commandId);
        public Task DeleteCommand(string commandId);
        public Task UpdateCommandStatus(string commandId,string commandStatus);
        public Task<Command?> GetCommandForOwner(string ownerId);
    }
}