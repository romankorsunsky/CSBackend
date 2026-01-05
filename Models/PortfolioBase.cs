using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace b1.Models
{
    public class PortfolioBase
    {
        [BsonId]
        private ObjectId Id;

        [BsonElement("ownerEmail")]
        [NotNull]
        private string OwnerEmail { get; set; } = null!;


    }
}