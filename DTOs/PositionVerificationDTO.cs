using b1.Models;

namespace b1.DTOs
{
    public class PositionVerificationDTO
    {
        public string Id { get; set; } = null!;
        public long Quantity { get; set; }
        public double Price { get; set; }
        public PositionVerificationDTO(PositionVerification verification)
        {
            Id = verification.Id;
            Quantity = verification.Quantity;
            Price = verification.Price;
        }
    }
}