using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace b1.Models
{
    [BsonDiscriminator(RootClass = true)]
    [BsonKnownTypes(typeof(RegularPortfolio),typeof(AdvancedPortfolio))]
    public abstract class Portfolio
    {

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = null!;

        [BsonElement]
        [NotNull]
        public string OwnerUsername { get; init; }

        [BsonElement]
        public string DisplayName { get; init; }

        public PortfolioStatus PtfStatus { get; init; }
        protected internal Portfolio(string username, string displayName, PortfolioStatus st = PortfolioStatus.ACTIVE)
        {
            OwnerUsername = username;
            DisplayName = displayName;
            PtfStatus = st;
        }
        public enum PortfolioStatus
        {
            ACTIVE,
            INACTIVE
        }
    }
    public class RegularPortfolio : Portfolio
    {
        public RegularPortfolio(string username, string displayName) : base(username, displayName)
        {
        }
    }
    public class AdvancedPortfolio : Portfolio
    {
        public AdvancedPortfolio(string username, string displayName) : base(username, displayName)
        {
        }
    }
}