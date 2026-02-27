using b1.Infrastructure;
using b1.Main;
using b1.Models;
using b1.Repositories;
using MongoDB.Driver;

namespace b1.Repositoris{
    public class MongoCommandRepo : ICommandRepository
    {
        private IMongoCollection<Command> _commands;
        public MongoCommandRepo(IMongoDatabase db)
        {
            _commands = db.GetCollection<Command>("posCommands");
            var indexModel = new CreateIndexModel<Command>(
                Builders<Command>.IndexKeys.Ascending(pc => pc.Id));
            _commands.Indexes.CreateOne(indexModel);
        }
        public async Task AddCommand(Command command)
        {
            await _commands.InsertOneAsync(command);
        }

        public async Task DeleteCommand(string commandId)
        {
            await _commands.DeleteOneAsync(cmd => cmd.Id == commandId);
        }

        public async Task<Command?> GetCommand(string commandId)
        {
            var res = await _commands.Find(cmd => cmd.Id == commandId).FirstOrDefaultAsync();
            return res;
        }

        public async Task<Command?> GetCommandForOwner(string ownerId)
        {
            var res = await _commands.Find(cmd => cmd.OwnerId == ownerId).FirstOrDefaultAsync();
            return res;
        }

        public async Task UpdateCommandStatus(string commandId,string commandStatus)
        {
            
            var update = Builders<Command>.Update.Set<string>(cmd => cmd.CmdStatus, commandStatus);
            var cmd = await _commands.FindOneAndUpdateAsync(cmd => cmd.Id == commandId, update);
        }
    }
}