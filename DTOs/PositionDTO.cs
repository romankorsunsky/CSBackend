using b1.Models;

namespace b1.DTOs
{
    public class PositionDTO
    {
        public string Id { get; set; }
        public string Symbol { get; set; }
        public string PositionType { get; set; }
        public double Price { get; set; }
        public long Quantity { get; set; }

        public PositionDTO(Position position)
        {
            Id = position.Id;
            PositionType = position.PositionType;
            Price = position.InitialPrice;
            Quantity = position.Quantity;
            Symbol = position.AssetSymbol;
        }
    }
}