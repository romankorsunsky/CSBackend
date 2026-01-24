using b1.Main;
using b1.Messages;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace b1.Services
{
    public class AssetService
    {

        private IMessageChannel _msgBroker;
        public AssetService(IMessageChannel broker)
        {
            _msgBroker = broker;
            _msgBroker.Subscribe<PriceChangedMsg>(HandlePriceChange);
        }

        public async Task HandlePriceChange(PriceChangedMsg msg)
        {
            Console.WriteLine(msg.Symbol + " changed to :" + msg.TimedPr.Price);
        }
        public async Task HandleNewAsset(NewAssetArrived msg)
        {
            
            return;
        }
    }
}