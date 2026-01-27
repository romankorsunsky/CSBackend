using System.Windows.Input;
using b1.Models;
using b1.Services;

namespace b1.Main
{
    public abstract class PortfolioReqProcessorBase
    {
        public abstract PositionMonitorBase ProcessPosition(Position position);

        public abstract ICommand CreateCommand(Position position, PositionMonitorBase monitor);

        public async Task Process(Portfolio portfolio,string username) {
            var positions = portfolio.
        }
    }
}