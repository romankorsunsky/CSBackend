using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace b1.Models
{
    public class Portfolio
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id;

        [BsonElement]
        [NotNull]
        public string OwnerUsername { get; set; } = null!;

        [BsonElement]
        public List<Position> Positions { get; init; } = null!;

        [BsonElement]
        [DefaultValue("active")]
        public string Status { get; set; } = null!;
    }
}