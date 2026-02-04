using System.Data.Common;
using System.Threading.Tasks;
using System.Windows.Input;
using b1.Models;
using b1.Services;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace b1.Main
{
    public abstract class PositionProcessorBase
    {
        private IMongoCollection<Position> _positionsCollection { get; init; }
        private IMongoCollection<PositionCommand> _commandsCollection { get; init; }
        private IMongoCollection<Portfolio> _portfoliosCollection { get; init; }
        protected internal PositionProcessorBase(IMongoDatabase db)
        {
            _positionsCollection = db.GetCollection<Position>("position");
            _commandsCollection = db.GetCollection<PositionCommand>("commands");
            _portfoliosCollection = db.GetCollection<Portfolio>("portfolios");

        }
        protected abstract Task<PositionCommand> CreateCommand(Position position,string username);
        //DO: create Portfolio object.
        public async Task<bool> Process(Position position, string username, IMongoClient client)
        {
            var res = await Task.Run(bool () => true);
            return res;
        }
    }
    public class RegularPositionProcessor : PositionProcessorBase
    {
        public RegularPositionProcessor(IMongoDatabase db) : base(db) { }

        protected override Task<PositionCommand> CreateCommand(Position position, string username)
        {
            PositionCommand res = new RegularPositionCommand(username,position);
            return Task.FromResult<PositionCommand>(res);
        }

    }
    public class AdvancedPositionProcessor : PositionProcessorBase
    {
        private IMongoDatabase _db;
        public AdvancedPositionProcessor(IMongoDatabase db) : base(db)
        {
            _db = db;
        }

        protected override async Task<PositionCommand> CreateCommand(Position position, string username)
        {
            var usersCollection = _db.GetCollection<User>("users");
            var user = await usersCollection.Find(usr => usr.Username == username).FirstOrDefaultAsync();
            if (user == null)
                throw new Exception("User does not exist");
            var email = user.Email;
            PositionCommand command = new AdvancedPositionCommand(email, username,position);
            return command;
        }
    }

    [BsonDiscriminator(RootClass = true)] //just in case, but it ensures mongo adds the _t discriminator
    //for proper deserialization of documnents to objects in the client
    public abstract class PositionCommand
    {
        [BsonId]
        public string Id { get; set; } = null!;

        [BsonElement]
        public string Username { get; set; } = null!;

        [BsonElement]
        public CommandStatus CmdStatus;

        [BsonElement]
        public string PositionId { get; set; } = null!;
        protected internal PositionCommand(string username, Position pos,CommandStatus status = CommandStatus.CLOSED)
        {
            PositionId = pos.Id;
            Username = username;
            CmdStatus = status;
        }
        public enum CommandStatus
        {
            ACTIVE,
            CLAIMED,
            CLOSED,
        }
    }
    [BsonKnownTypes(typeof(AdvancedPositionCommand))]
    public class AdvancedPositionCommand : PositionCommand
    {
        public AdvancedPositionCommand(string notifEmail, string username,Position pos, CommandStatus status = CommandStatus.CLOSED)
         : base(username,pos, status)
        {
            NotificationEmail = notifEmail;
        }
        [BsonElement]
        public string NotificationEmail { get; init; }
    }
    [BsonKnownTypes(typeof(RegularPositionCommand))]
    public class RegularPositionCommand : PositionCommand
    {
        public RegularPositionCommand(string username,Position pos, CommandStatus status = CommandStatus.CLOSED)
         : base(username, pos, status) { }

        public string LoggingMessage = "Closed Position ";
    }
}