using System.Collections.Concurrent;
using b1.Models;
using MongoDB.Bson;

namespace b1.Main
{
    public class PriceContext
    {
        private readonly string _type;

        public ConcurrentDictionary<string, TimedPrice> PricesForSymbol { get; init; } = null!;

        public PriceContext(string type)
        {
            _type = type;
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
            if (PricesForSymbol.TryGetValue(symbol,out var val))
            {
                return val;
            }
            return null;
        }
        public string GetAssetType()
        {
            return _type;
        }
        public void GetSnapShot()
        {
            var keys = PricesForSymbol.Keys.ToList();
            var str = "";
            keys.ForEach((symbol) => { str = str + "{" + symbol + "," + PricesForSymbol[symbol].ToJson() + "},"; });
            Console.Write(_type + "[");
            Console.Write(str);
            Console.Write("]");
        }
        public List<string> GetSymbolNames()
        {
            return PricesForSymbol.Keys.ToList();
        }
    }
}