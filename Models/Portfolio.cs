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

        public abstract string PortfolioType();
    }
    
    [BsonKnownTypes(typeof(RegularPortfolio))]
    public class RegularPortfolio : Portfolio
    {
        public RegularPortfolio(string ownerId, string displayName)
         : base(ownerId, displayName)
        {
        }

        public override string PortfolioType()
        {
            return "REGULAR";
        }
    }
    [BsonKnownTypes(typeof(AdvancedPortfolio))]
    public class AdvancedPortfolio : Portfolio
    {
        public AdvancedPortfolio(string ownerId, string displayName)
         : base(ownerId, displayName)
        {
        }

        public override string PortfolioType()
        {
            return "ADVANCED";
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
            PortfolioType = portfolio.PortfolioType();
        }
        public override string ToString()
        {
            return this.ToJson();
        }
    }
}