using MongoDB.Bson.Serialization.Attributes;

namespace b1.Models
{
    public class PositionVerification
    {
        [BsonId]
        public string Id { get; set; } = null!;
        public string Symbol { get; set; } = null!;
        public long Quantity { get; set; }
        public double Price { get; set; }
        public string TimeIssued { get; set; } = null!;
        public string PortfolioId { get; set; } = null!;
        public string PositionType { get; set; } = null!;
        public PositionVerification(string symbol, long quantity, double price,
            string timeIssued, string ptfId, string posType)
        {
            Id = Guid.NewGuid().ToString();
            Symbol = symbol;
            Quantity = quantity;
            Price = price;
            TimeIssued = timeIssued;
            PortfolioId = ptfId;
            PositionType = posType;
        }
    }
}
