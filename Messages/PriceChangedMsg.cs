using b1.Models;

namespace b1.Messages
{
    public struct PriceChangedMsg : IMessage
    {
        public string Symbol { get; set; }
        public TimedPrice TimedPr { get; set; }
        public PriceChangedMsg(string sym, TimedPrice  tp)
        {
            Symbol = sym;
            TimedPr = tp;
        }
    }
}