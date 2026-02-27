using System.Collections.Concurrent;
using b1.Infrastructure;
using b1.Messages;
using b1.Models;
using MongoDB.Bson;

namespace b1.Main
{
    public class PriceContext
    {

        private ConcurrentDictionary<string, TimedPrice> PricesForSymbol { get; init; } = null!;
        private readonly IMessageChannel _msgBroker;
        public PriceContext(IMessageChannel broker)
        {
            _msgBroker = broker;
            broker.SubscribeToEvent<PriceChangedMsg>(PriceUpdateHandler);
            PricesForSymbol = new ConcurrentDictionary<string, TimedPrice>();
        }

        public async Task PriceUpdateHandler(PriceChangedMsg e)
        {
            await Task.Run(() =>
            {
                var tp = e.TimedPr;
                var symbol = e.Symbol;
                if (symbol != null && tp.Price >= 0)
                    PricesForSymbol.AddOrUpdate(symbol, tp, (symbol, prev) => tp);
            });   
        }
        public TimedPrice? GetTimedPrice(string symbol)
        {   
            if (PricesForSymbol.TryGetValue(symbol, out var val))
            {
                return val;
            }
            return null;
        }
        public List<string> GetSymbolNames()
        {
            return PricesForSymbol.Keys.ToList();
        }
    }
}