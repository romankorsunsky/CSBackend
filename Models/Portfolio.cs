using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace b1.Models
{
    public abstract class Portfolio
    {

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id;

        [BsonElement]
        [NotNull]
        public string OwnerUsername { get; init; }

        [BsonElement]
        public string DisplayName { get; init; }
    }
    public class RegularPortfolio : Portfolio { }
    public class AdvancedPortfolio: Portfolio { }
}