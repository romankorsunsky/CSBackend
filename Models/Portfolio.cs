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

        [BsonElement]
        public string PtfType { get; init; }
        protected internal Portfolio(string ownerid,
            string displayName,
            string pt,
            string st = PortfolioStatus.ACTIVE)
        {
            OwnerId = ownerid;
            DisplayName = displayName;
            PtfStatus = st;
            PtfType = pt;
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
        public RegularPortfolio(string ownerId, string displayName, string pt = PortfolioType.REGULAR)
         : base(ownerId, displayName,pt)
        {
        }
    }
    [BsonKnownTypes(typeof(AdvancedPortfolio))]
    public class AdvancedPortfolio : Portfolio
    {
        public AdvancedPortfolio(string ownerId, string displayName, string pt = PortfolioType.ADVANCED)
         : base(ownerId, displayName, pt)
        {
        }
    }
    public class PortfolioDTO
    {
        public string Id { get; init; }
        public string DisplayName { get; init; }
        public List<PositionDTO> Positions { get; init; }
        public string PortfolioType { get; init; }

        [JsonIgnore] //just in case
        public static Dictionary<Type, Func<object, Portfolio>> portfolioTypeMap =
            new Dictionary<Type, Func<object, Portfolio>>();
        public PortfolioDTO(Portfolio portfolio, List<PositionDTO> positions)
        {
            Id = portfolio.Id;
            DisplayName = portfolio.DisplayName;
            Positions = positions;
            PortfolioType = portfolio.PtfType;
        }
        public override string ToString()
        {
            return this.ToJson();
        }
    }
}