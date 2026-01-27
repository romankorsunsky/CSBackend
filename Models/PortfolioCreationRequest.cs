namespace b1.Models
{
    public class PortfolioCreationRequest
    {
        public string DisplayName { get; set; }
        public List<Position> Positions { get; set; }

        public string PortfolioType { get; set; }
    }
}