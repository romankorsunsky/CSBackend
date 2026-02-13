using Microsoft.OpenApi.Extensions;
using MongoDB.Bson.Serialization.Attributes;

namespace b1.Models
{
    public class PortfolioCreationRequest
    {
        public string DisplayName { get; set; }

        public string PtfType { get; set; }
        public PortfolioCreationRequest(string displayName, string ptfType)
        {
            DisplayName = displayName;
            PtfType = ptfType;
        }
    }
}