using b1.Main;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace b1.Models
{
    public class TickerData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id;

        public string Symbol { get; set; } = null!;

        public string LongName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string TickerType { get; set; } = null!;


        public TickerData() { }
        public TickerData(TickerBaseData baseData, string type)
        {
            Description = (baseData.Description == null || baseData.Description == "") ? "N/A" : baseData.Description;
            Symbol = baseData.Name ?? throw new ArgumentException("Bad Ticker Info");
            LongName = baseData.LongName ?? throw new ArgumentException("Bad Ticker Info");
            TickerType = type;

        }

        public override string ToString()
        {
            return $"TickerData: [Symbol:{Symbol};LongName:{LongName};Description:{Description};Type:{TickerType}]";
        }
    }
}