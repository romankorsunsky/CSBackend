using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace b1.Models
{
    public class Position
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        public string PortfolioId { get; set; } = null!;
        [BsonElement]
        public string AssetSymbol { get; init; } = null!;

        [BsonElement]
        public long Quantity { get; init; }

        [BsonElement]
        public DateTime OpenedAt { get; init; }

        [BsonElement]
        public double InitialPrice { get; init; }

        [BsonElement]
        public string PositionType { get; init; }

        [BsonElement]
        public bool Closed { get; init; }

        public Position(string portfolioId, string assetSymbol, long qtty, DateTime openedAt, double initPrice, string posType)
        {
            Closed = false;
            PortfolioId = portfolioId;
            AssetSymbol = assetSymbol;
            Quantity = qtty;
            OpenedAt = openedAt;
            InitialPrice = initPrice;
            PositionType = posType;
        }
    }
    public struct PositionDirection
    {
        public const string LONG = "LONG";
        public const string SHORT = "SHORT";
    }
}


