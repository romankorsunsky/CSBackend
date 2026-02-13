using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace b1.Models
{
    [BsonDiscriminator(RootClass = true)]
    public abstract class AssetEOD
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id;

        [BsonElement("symbol")]
        public string? Symbol { get; init; }

        [BsonElement("date")]
        public DateTime Date { get; init; }

        [BsonElement("open")]
        public double Open { get; init; }

        [BsonElement("close")]
        public double Close { get; init; }

        [BsonElement("low")]
        public double Low { get; init; }

        [BsonElement("high")]
        public double High { get; init; }

        public AssetEOD() { }
        public AssetEOD(string shortName, DateTime date, double open, double close, double low, double high)
        {
            Symbol = shortName;
            Date = date;
            Open = open;
            Close = close;
            High = high;
            Low = low;
        }
    }
}