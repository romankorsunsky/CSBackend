using System.Collections.Concurrent;
using b1.Models;
using MongoDB.Bson;

namespace b1.Main
{
    public class PriceContext
    {
        public string AssetType { get; init; }

        public ConcurrentDictionary<string, TimedPrice> PricesForSymbol { get; init; } = null!;

        public PriceContext(string type)
        {
            AssetType = type;
            PricesForSymbol = new ConcurrentDictionary<string, TimedPrice>();
        }

        //add a <symbol name, asset price> entry, if the symbol doesn't exist, adds it.
        public void PutPrice(string symbol, TimedPrice tp)
        {
            if (symbol != null && tp.Price >= 0)
                PricesForSymbol.AddOrUpdate(symbol, tp, (symbol, prev) => tp);
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