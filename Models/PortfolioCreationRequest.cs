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
    //this is not elegant, the alternative was separate classes and different endpoints for each type, which sounded even worse
    public struct PortfolioType
    {
        public const string REGULAR = "REGULAR";
        public const string ADVANCED = "ADVANCED";
    }
}