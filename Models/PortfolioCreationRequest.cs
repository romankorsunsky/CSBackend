using MongoDB.Bson.Serialization.Attributes;

namespace b1.Models
{
    public abstract class PortfolioCreationRequest
    {
        [BsonId]
        public string Id { get; set; } = null!;
        [BsonElement]
        public string DisplayName { get; set; }
        [BsonElement]
        public List<Position> Positions { get; set; }
        [BsonElement]
        public PortfolioType PtfType { get; set; }
        public bool Processed { get; set; }
        public PortfolioCreationRequest(string displayName, PortfolioType ptfType, bool processed = false)
        {
            DisplayName = displayName;
            Positions = new();
            Processed = processed;
            PtfType = ptfType;
        }
        public enum PortfolioType
        {
            REGULAR,
            ADVANCED
        }
    }
}