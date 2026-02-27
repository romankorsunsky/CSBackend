using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace b1.Models
{
    public abstract class Portfolio
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        [BsonElement]
        [NotNull]
        public string OwnerId { get; init; }

        [BsonElement]
        public string DisplayName { get; init; }

        [BsonElement]
        public string PtfStatus { get; init; }

        public abstract string PortfolioType { get;}
        protected internal Portfolio(string ownerid,
            string displayName,
            string st = PortfolioStatus.ACTIVE)
        {
            OwnerId = ownerid;
            DisplayName = displayName;
            PtfStatus = st;
        }
        public struct PortfolioStatus
        {
            public const string ACTIVE = "ACTIVE";
            public const string INACTIVE = "INACTIVE";
        }
    }
    
    [BsonKnownTypes(typeof(RegularPortfolio))]
    public class RegularPortfolio : Portfolio
    {
        public RegularPortfolio(string ownerId, string displayName)
         : base(ownerId, displayName)
        {
        }

        public override string PortfolioType
        {
            get => "REGULAR";
        }
    }
    [BsonKnownTypes(typeof(AdvancedPortfolio))]
    public class AdvancedPortfolio : Portfolio
    {
        public AdvancedPortfolio(string ownerId, string displayName)
         : base(ownerId, displayName)
        {
        }

        public override string PortfolioType
        {
            get => "ADVANCED";
        }
    }
}