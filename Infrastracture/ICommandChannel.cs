using b1.Main;
using b1.Repositories;

namespace b1.Infrastructure
{
    public interface ICommandChannel
    {
        public void AssignHandlerToCommand(Type cmdType, ICommandHandlerProvider cmdProv);
        public Task ExecuteCommand(Command command);
    }

    public interface ICommandHandlerProvider
    {
        public ICommandHandler GetHandler();
    }
    public class RegularPositionCommandProvider : ICommandHandlerProvider
    {
        private ICommandChannel _cmdChan;
        private ICommandRepository _cmdRepo;

        public RegularPositionCommandProvider(ICommandChannel commandChannel,
            ICommandRepository commandRepo)
        {
            _cmdChan = commandChannel;
            _cmdRepo = commandRepo;
            _cmdChan.AssignHandlerToCommand(typeof(RegularPositionCommand), this);
        }
        public ICommandHandler GetHandler()
        {
            return new RegularPositionCommandHandler(_cmdRepo);
        }
    }
    public class AdvancedPositionCommandProvider : ICommandHandlerProvider
    {
        private ICommandChannel _cmdChan;
        private ICommandRepository _cmdRepo;
        private IUserRepository _usrRepo;

        public AdvancedPositionCommandProvider(ICommandChannel commandChannel,
            ICommandRepository commandRepo, IUserRepository usrRepo)
        {
            _cmdChan = commandChannel;
            _cmdRepo = commandRepo;
            _usrRepo = usrRepo;
            _cmdChan.AssignHandlerToCommand(typeof(AdvancedPositionCommand), this);
        }
        public ICommandHandler GetHandler()
        {
            return new AdvancedPositionCommandHandler(_cmdRepo, _usrRepo);
        }
    }

}