using b1.Models;

namespace b1.Messages
{
    public struct PriceChangedMsg : IMessage
    {
        public string Symbol { get; init; }
        public TimedPrice TimedPr { get; init; }
        public PriceChangedMsg(string sym, TimedPrice  tp)
        {
            Symbol = sym;
            TimedPr = tp;
        }
    }
}