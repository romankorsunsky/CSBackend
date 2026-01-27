

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace b1.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement("username")]
        public string Username { get; set; } = null!;
        [BsonElement("email")]
        public string Email { get; set; } = null!;

        [BsonElement("firstName")]
        public string Fname { get; set; } = null!;

        [BsonElement("lastName")]
        public string Lname { get; set; } = null!;

        [BsonElement("password")]
        public string Password { get; set; } = null!;

        [BsonElement("portfolio")]
        public string UserPortfolioId { get; set; } = null!;
    }
}