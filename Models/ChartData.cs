using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace b1.Models
{
    public class ChartData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id;

        [BsonElement("symbol")]
        public string Symbol { get; set; } = null!;

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
        public ChartData(){}
        public ChartData(string symbol)
        {
            Symbol = symbol;
            //regarding the sizes
            LastDayPrices = new List<TimedPrice>(); //once every 12 minutes, 24 * (60/12) = 24 * 5 = 120
            LastTwoWeekPrices = new List<TimedPrice>(); //once every 2 hours 
            LastTwoMonthPrices = new List<TimedPrice>(); //every day
            LastYearPrices = new List<TimedPrice>(); //365 / 5, every 5 days
            LastFiveYearPrices = new List<TimedPrice>(); //same but every 25 days (365 * 5)/(5*5)
        }

    }
}