namespace b1.Messages
{
    public struct PriceChangedMsg : IMessage
    {
        public string Symbol { get; set; }
        public double Price { get; set; }

        public DateTime AtTime { get; set; }
        public PriceChangedMsg(string sym, double price, DateTime at)
        {
            Symbol = sym;
            Price = price;
            AtTime = at;
        }
    }
}