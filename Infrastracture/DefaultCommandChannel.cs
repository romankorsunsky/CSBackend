
using System.Collections.Concurrent;
using b1.Main;
using b1.Models;
using b1.Repositories;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using static b1.Main.PositionCommand;

namespace b1.Infrastructure
{
    public class DefaultCommandChannel : ICommandChannel
    {
        private ConcurrentDictionary<Type, ICommandHandlerProvider> _handlers = new();

        public void AssignHandlerToCommand(Type tp, ICommandHandlerProvider provider)
        {
            if (!_handlers.TryAdd(tp, provider))
            {
                throw new Exception("Failed to assign handler to command" + tp);
            }
        }

        public async Task ExecuteCommand(Command command)
        {
            var cmdType = command.GetType();
            if (command is null)
            {
                throw new Exception("Command is null");
            }
            if (_handlers.TryGetValue(cmdType, out var handler))
            {
                await handler.GetHandler().Handle(command);
            }
            else
            {
                throw new Exception("No handler for type " + cmdType.ToString());
            }
        }
    }
    public interface ICommandHandler
    {
        public Task Handle(object cmd);
    }

    public abstract class Command
    {
        [BsonId]
        public string Id { get; set; } = null!;
        public string OwnerId { get; set; } = null!;
        public string CmdStatus { get; set; } = null!;
        protected Command(string status, string ownerId)
        {
            Id = Guid.NewGuid().ToString();
            CmdStatus = status;
            OwnerId = ownerId;
            Console.WriteLine("OWNER ID = " + ownerId);
        }
    }

    public class RegularPositionCommandHandler: ICommandHandler
    {
        
        private ICommandRepository _commandRepo;
        public RegularPositionCommandHandler(ICommandRepository cmdRepo)
        {
            _commandRepo = cmdRepo;
        }
        private async Task Handle(RegularPositionCommand command)
        {
            try
            {
                if (!(command.CmdStatus == CommandStatus.ACTIVE))
                {
                    return;
                }
                Console.WriteLine(command.LoggingMessage);
                await _commandRepo.DeleteCommand(command.Id);
                Console.WriteLine("[LOG] Finished processing command");
            }
            catch (Exception)
            {
                Console.WriteLine("[LOG] Some error occured when exuecuting command, retrying...");
                //add retry option later
            }
        }

        public async Task Handle(object cmd)
        {
            await Handle((RegularPositionCommand)cmd);
        }
    }
    public class AdvancedPositionCommandHandler: ICommandHandler
    {
        private IUserRepository _userRepo;
        private ICommandRepository _cmdRepo;
        public AdvancedPositionCommandHandler(ICommandRepository cmdRepo,
            IUserRepository userRepo)
        {
            _userRepo = userRepo;
            _cmdRepo = cmdRepo;
        }
        public async Task Handle(AdvancedPositionCommand command)
        {
            try
            {
                var notifEmail = command.NotificationEmail;
                var userId = command.UserId;
                User? user = await _userRepo.GetUserById(userId);
                if (user == null)
                {
                    throw new Exception("Didn't find user for command");
                }
                // Let's imagine here I set an email
                Console.WriteLine("[LOG] sent email to: " + notifEmail);
                // Also imagine I've done something with the userId.
                // The point of those handlers is just to show handling of Command objects
                // that are self contained.
                await _cmdRepo.DeleteCommand(command.Id);
                Console.WriteLine("[LOG] Finished processing command");
            }
            catch (Exception)
            {
                Console.WriteLine("[LOG] Some error occured when exuecuting command, retrying...");
                //implement retry later
            }
            
        }
        public async Task Handle(object cmd)
        {
            await Handle((AdvancedPositionCommand)cmd);
        }
    }
}