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
        [BsonElement("symbol")]
        public string Symbol { get; set; } = null!;

        [BsonElement("longName")]
        public string LongName { get; set; } = null!;

        [BsonElement("description")]
        public string Description { get; set; } = null!;

        [BsonElement("type")]
        public string TickerType { get; set; } = null!;

        [BsonElement("oneday")]
        public IList<TimedPrice> LastDayPrices { get; set; } = null!;

        [BsonElement("twoweek")]
        public IList<TimedPrice> LastTwoWeekPrices { get; set; } = null!;

        [BsonElement("twomonth")]
        public IList<TimedPrice> LastTwoMonthPrices { get; set; } = null!;

        [BsonElement("oneyear")]
        public IList<TimedPrice> LastYearPrices { get; set; } = null!;

        [BsonElement("fiveyear")]
        public IList<TimedPrice> LastFiveYearPrices { get; set; } = null!;

        private TickerData()
        {
            //regarding the sizes
            LastDayPrices = new List<TimedPrice>(); //once every 12 minutes, 24 * (60/12) = 24 * 5 = 120
            LastTwoWeekPrices = new List<TimedPrice>(); //once every 2 hours 
            LastTwoMonthPrices = new List<TimedPrice>(); //every day
            LastYearPrices = new List<TimedPrice>(); //365 / 5, every 5 days
            LastFiveYearPrices = new List<TimedPrice>(); //same but every 25 days (365 * 5)/(5*5)
        }
        public TickerData(TickerBaseData baseData, string type) : this()
        {

            Description = (baseData.Description == null || baseData.Description == "") ? "N/A" : baseData.Description;
            Symbol = baseData.Name ?? throw new ArgumentException("Bad Ticker Info");
            LongName = baseData.LongName ?? throw new ArgumentException("Bad Ticker Info");
            TickerType = type;

        }
    }
}