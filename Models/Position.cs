using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace b1.Models
{
    public sealed class Position
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id;
        [BsonElement]
        public string AssetSymbol { get; init; } = null!;

        [BsonElement]
        public long Quantity { get; init; }

        [BsonElement]
        public DateTime OpenedAt { get; init; }

        [BsonElement]
        public double InitialPrice { get; init; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement]
        public Direction PositionType { get; init; }

        [BsonElement]
        public bool Closed { get; init; }
    }
    public enum Direction
    {
        LONG,
        SHORT
    }
}


