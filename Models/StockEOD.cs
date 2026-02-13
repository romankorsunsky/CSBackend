using MongoDB.Bson.Serialization.Attributes;

namespace b1.Models
{
    [BsonKnownTypes(typeof(StockEOD))]
    public class StockEOD : AssetEOD
    {

        [BsonElement("volume")]
        public int Volume { get; init; }

        [BsonElement("dividends")]
        public double Dividends { get; init; }

        [BsonElement("splits")]
        public double StockSplits { get; init; }

        public StockEOD(string shortName,DateTime date, double open, double high, double close, double low, int volume, double divs, double splits) :
        base(shortName,date, open, close, low, high)
        {
            Volume = volume;
            Dividends = divs;
            StockSplits = splits;
        }
    }
}