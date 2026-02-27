using System.Text.Json.Serialization;
using b1.Models;
using MongoDB.Bson;

namespace b1.DTOs {
    public class PortfolioDTO
    {
        public string Id { get; init; }
        public string DisplayName { get; init; }
        public List<PositionDTO> Positions { get; init; }
        public string PortfolioType { get; init; }

        [JsonIgnore]
        public static Dictionary<Type, Func<object, Portfolio>> portfolioTypeMap =
            new Dictionary<Type, Func<object, Portfolio>>();
        public PortfolioDTO(Portfolio portfolio, List<PositionDTO> positions)
        {
            Id = portfolio.Id;
            DisplayName = portfolio.DisplayName;
            Positions = positions;
            PortfolioType = portfolio.PortfolioType;
        }
        public override string ToString()
        {
            return this.ToJson();
        }
    }
}