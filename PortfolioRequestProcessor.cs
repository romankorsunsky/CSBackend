using System.Data.Common;
using System.Threading.Tasks;
using System.Windows.Input;
using b1.Models;
using b1.Services;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace b1.Main
{
    public abstract class PortfolioReqProcessorBase
    {
        private IMongoCollection<Position> _positionsCollection { get; init; }
        private IMongoCollection<PositionCommand> _commandsCollection { get; init; }
        private IMongoCollection<PositionMonitor> _monitorsCollection { get; init; }
        private IMongoCollection<Portfolio> _portfoliosCollection { get; init; }
        protected internal PortfolioReqProcessorBase(IMongoDatabase db)
        {
            _positionsCollection = db.GetCollection<Position>("position");
            _commandsCollection = db.GetCollection<PositionCommand>("commands");
            _monitorsCollection = db.GetCollection<PositionMonitor>("monitors");
            _portfoliosCollection = db.GetCollection<Portfolio>("portfolios");

        }
        public abstract Task<PositionCommand> CreateCommand(Position position,string username);
        public abstract Portfolio CreatePortfolio(string username,string displayName);

        //DO: create Portfolio object.
        public async Task Process(PortfolioCreationRequest req, string username)
        {
            if (req.DisplayName == null || req.DisplayName.StartsWith(" ") ||
                req.Positions == null || req.Positions.Count == 0)
            {
                throw new ArgumentNullException("Fix your god dam' request ! * stares in Samuel L. Jackson's stare *");
            }
            // create a Portfolio and Positions
            var portfolio = CreatePortfolio(username, req.DisplayName);
            await _portfoliosCollection.InsertOneAsync(portfolio);
            var portfolioId = portfolio.Id;
            if (portfolioId == null)
                throw new Exception("MongoDriver failed to init Id properly");
            foreach (var p in req.Positions)
            {
                //here would be the place to validate each position, calculate total
                //price of portfolio and check with user's balance etc, for lack of time i iwll skip it
                p.PortfolioId = portfolioId;
            }
            await _positionsCollection.InsertManyAsync(req.Positions);
            List<PositionCommand> commands = new List<PositionCommand>();
            List<PositionMonitor> monitors = new List<PositionMonitor>();
            //create Commands and PriceMonitors
            //remember, now we will be creating Commands and Monitors, 
            foreach (var p in req.Positions)
            {
                var command = await CreateCommand(p, username);
                commands.Add(command);
            }
            await _commandsCollection.InsertManyAsync(commands);
            if (commands.Count != req.Positions.Count)
                throw new Exception("command per positon, must be 1-to-1");
            for (int i = 0; i <= commands.Count; i++)
            {
                var position = req.Positions[i];
                if (FilteredPosition(position) == false)
                    continue;
                var monitor = CreatePosition(position,commands[i].Id);
                monitors.Add(monitor);
            }
            await _monitorsCollection.InsertManyAsync(monitors);
        }

        protected internal abstract PositionMonitor CreatePosition(Position position,string commandId);

        protected internal abstract bool FilteredPosition(Position position);
    }
    public class RegularPortfolioRequestProcessor : PortfolioReqProcessorBase
    {
        public RegularPortfolioRequestProcessor(IMongoDatabase db) : base(db) { }

        public override Task<PositionCommand> CreateCommand(Position position, string username)
        {
            PositionCommand res = new RegularPositionCommand(username);
            return Task.FromResult<PositionCommand>(res);
        }

        public override Portfolio CreatePortfolio(string username, string displayName)
        {
            return new RegularPortfolio(username, displayName);
        }

        protected internal override PositionMonitor CreatePosition(Position position, string commandId)
        {
            double triggerPrice = position.InitialPrice * 0.95; //rounding errors will accumulate, should change to Decimal
            return new PositionMonitor(position.Id, commandId, position.AssetSymbol, triggerPrice,
                PositionMonitor.MonitorCondition.NEW_IS_HIGHER);
        }
        // for a regular portfolio, we aggresively cut losses on SHORT position
        // we don't track losses on LONG position, as we don't incur the losses anyway
        protected internal override bool FilteredPosition(Position position)
        {
            return position.PositionType == Direction.SHORT;
        }
    }
    public class AdvancedPortfolioRequestProcessor : PortfolioReqProcessorBase
    {
        private IMongoDatabase _db;
        public AdvancedPortfolioRequestProcessor(IMongoDatabase db) : base(db)
        {
            _db = db;
        }

        public override async Task<PositionCommand> CreateCommand(Position position, string username)
        {
            var usersCollection = _db.GetCollection<User>("users");
            var user = await usersCollection.Find(usr => usr.Username == username).FirstOrDefaultAsync();
            if (user == null)
                throw new Exception("User does not exist");
            var email = user.Email;
            PositionCommand command = new AdvancedPositionCommand(email, username);
            return command;
        }

        public override Portfolio CreatePortfolio(string username, string displayName)
        {
            return new AdvancedPortfolio(username, displayName);
        }

        protected internal override bool FilteredPosition(Position position)
        {
            return true;
        }
        protected internal override PositionMonitor CreatePosition(Position position, string commandId)
        {
            if (position is AdvancedPosition advancedPosition)
            {
                var cond = position.PositionType == Direction.LONG
                 ? PositionMonitor.MonitorCondition.NEW_IS_LOWER :
                    PositionMonitor.MonitorCondition.NEW_IS_HIGHER;

                return new PositionMonitor(position.Id, commandId, position.AssetSymbol, advancedPosition.TriggerPrice,
                    cond);
            }
            throw new Exception("Position should be AdvancedPosition in " + this.GetType().ToString());
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

        protected internal PositionCommand(string username, CommandStatus status = CommandStatus.CLOSED)
        {
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
        public AdvancedPositionCommand(string notifEmail, string username, CommandStatus status = CommandStatus.CLOSED)
         : base(username, status)
        {
            NotificationEmail = notifEmail;
        }
        [BsonElement]
        public string NotificationEmail { get; init; }
    }
    [BsonKnownTypes(typeof(RegularPositionCommand))]
    public class RegularPositionCommand : PositionCommand
    {
        public RegularPositionCommand(string username, CommandStatus status = CommandStatus.CLOSED)
         : base(username, status) { }

        public string MessageToPrint = "Closed Position ";
    }
}