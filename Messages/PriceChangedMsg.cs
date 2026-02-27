using b1.Models;

namespace b1.Messages
{
    /// <summary>
    /// I want to add price trackers later for automatic exchange, so I need
    /// a mechanism to deliver a price change to relevant subscribers, pair this with a messageBus(broker).
    /// </summary>
    public struct PriceChangedMsg : IMessage
    {
        public string Symbol { get; init; }
        public TimedPrice TimedPr { get; init; }
        public PriceChangedMsg(string sym, TimedPrice tp)
        {
            Symbol = sym;
            TimedPr = tp;
        }
    }
}