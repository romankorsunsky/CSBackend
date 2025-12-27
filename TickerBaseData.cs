

using MongoDB.Bson.Serialization.Attributes;

namespace b1.Main
{
    public class TickerBaseData
    {
        public string? Description { get; set; }
        
        public string? Name { get; set; }

        public string? LongName { get; set; }

    }
}