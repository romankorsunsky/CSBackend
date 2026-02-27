using System.Collections.Concurrent;
using System.Data.Common;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Input;
using b1.DTOs;
using b1.Infrastructure;
using b1.Models;
using b1.Repositories;
using b1.Respositories;
using b1.Services;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace b1.Main
{
    public abstract class CommandCreatorBase
    {
        private IUserRepository _userRepo { get; set; }
        private IPositionRepository _posRepo { get; init; }
        private ICommandRepository _commandRepo { get; init; }
        private IPortfolioRepository _portfolioRepo { get; init; }
        protected internal CommandCreatorBase(
            IUserRepository userRepo,
            IPositionRepository posRepo,
            IPortfolioRepository portfRepo,
            ICommandRepository commandRepo)
        {
            _userRepo = userRepo;
            _posRepo = posRepo;
            _commandRepo = commandRepo;
            _portfolioRepo = portfRepo;

        }
        protected abstract Task<PositionCommand> CreateCommand(Position position, User user);

        /// <summary>
        /// A processor object that processes a Position that is being opened.
        /// </summary>
        /// <param name="position">Position to process</param>
        /// <returns>A PositionDTO on sucess, null on failure</returns>
        public async Task<PositionCommand?> TryAddCommand(Position position,User user)
        {
            try
            {
                PositionCommand command = await CreateCommand(position, user);
                await _commandRepo.AddCommand(command);
                return command;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
    public class RegularCommandCreator : CommandCreatorBase
    {
        public RegularCommandCreator(
            IUserRepository userRepo,
            IPositionRepository posRepo,
            IPortfolioRepository portfRepo,
            ICommandRepository commandRepo) : base(userRepo, posRepo, portfRepo, commandRepo) { }

        protected override Task<PositionCommand> CreateCommand(Position position, User user)
        {
            PositionCommand res = new RegularPositionCommand(user, position);
            return Task.FromResult<PositionCommand>(res);
        }

    }
    public class AdvancedCommandCreator : CommandCreatorBase
    {
        public AdvancedCommandCreator(
            IUserRepository userRepo,
            IPositionRepository posRepo,
            IPortfolioRepository portfRepo,
            ICommandRepository commandRepo) : base(userRepo, posRepo, portfRepo, commandRepo) { }

        protected override Task<PositionCommand> CreateCommand(Position position, User user)
        {
            var userId = user.Id;
            var email = user.Email;
            PositionCommand command = new AdvancedPositionCommand(email, userId, position);
            return Task.FromResult(command);
        }
    }

    public abstract class PositionCommand: Command
    {
        protected internal PositionCommand(Position pos, string status = CommandStatus.INACTIVE)
        : base(status,pos.Id) { }
        public struct CommandStatus
        {
            public const string ACTIVE = "ACTIVE";
            public const string INACTIVE = "INACTIVE";
            public const string CLAIMED = "CLAIMED";
        }
    }
    [BsonKnownTypes(typeof(AdvancedPositionCommand))]
    public class AdvancedPositionCommand : PositionCommand
    {
        public string NotificationEmail { get; init; }
        public string UserId { get; set; }
        public AdvancedPositionCommand(string notifEmail, string userId, Position pos,
            string status = CommandStatus.INACTIVE)
            : base(pos, status)
        {
            NotificationEmail = notifEmail;
            UserId = userId;
        }

    }
    [BsonKnownTypes(typeof(RegularPositionCommand))]
    public class RegularPositionCommand : PositionCommand
    {
        public string LoggingMessage = "Closed Position";
        public RegularPositionCommand(User user, Position pos,
            string status = CommandStatus.INACTIVE)
            : base(pos, status) { }
    }

    [Serializable]
    internal class WriteException : Exception
    {
        public WriteException() { }

        public WriteException(string? message) : base(message) { }

        public WriteException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}