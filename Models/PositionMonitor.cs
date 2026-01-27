using MongoDB.Bson.Serialization.Attributes;

namespace b1.Models
{
    public class PositionMonitorBase
    {
        [BsonId]
        public string Id;

        [BsonElement]
        public string PostionId;

        [BsonElement]
        public string CommandId;
    }
}