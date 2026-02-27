namespace b1.Models{
    public class PositionCreationRequest
    {
        public string Symbol { get; set; } = null!;
        public string PositionType { get; set; } = null!;
        public long Quantity { get; set; }
    }
}