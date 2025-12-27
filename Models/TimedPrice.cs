
using MongoDB.Bson.Serialization.Attributes;

namespace b1.Models
{
    public class TimedPrice
    {
        [BsonElement("price")]
        public double Price { get; init; } = default;

        [BsonElement("date")]
        public DateTime Date { get; init; } = default;

        public TimedPrice(){}
        public TimedPrice(double price, DateTime date)
        {
            Price = price;
            Date = date;
        }
    }
}