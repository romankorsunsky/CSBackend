using MongoDB.Bson.Serialization.Attributes;

namespace b1.Models
{
    public class AdvancedPosition : Position
    {
        [BsonElement]
        public double TriggerPrice { get; set; }
    }
}