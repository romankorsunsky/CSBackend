
using MongoDB.Bson.Serialization.Attributes;

namespace b1.Models
{
    public class TimedPrice
    {
        [BsonElement("price")]
        public double Price { get; init; } = default;

        [BsonElement("date")]
        public DateTime Date { get; init; } = default;

        public TimedPrice() { }
        public TimedPrice(double price, DateTime date)
        {
            Price = price;
            Date = date;
        }
    }
    public class TimedPriceWithSymbol : TimedPrice
    {
        [BsonElement]
        public string Symbol { get; set; }
        public TimedPriceWithSymbol(double price, DateTime date, string symbol)
            : base(price, date)
        {
            Symbol = symbol;
        }
    }
}