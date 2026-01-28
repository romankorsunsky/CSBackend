using b1.Services;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace b1.Models
{
    [BsonKnownTypes(typeof(PositionMonitor))]
    public class PositionMonitor
    {
        [BsonId]
        public string Id { get; set; } = null!;

        [BsonElement]
        public string PostionId { get; set; }
        [BsonElement]
        public string Symbol { get; set; }
        [BsonElement]
        public string CommandId { get; set; }
        [BsonElement]
        public double TriggerPrice { get; set; }
        [BsonElement]
        public MonitorCondition Condition{ get; set; }
        [BsonElement]
        public MonitorStatus MntrStatus { get; set; }
        public PositionMonitor(string positionId, string commandId, string symbol,
             double triggerPrice,MonitorCondition cond,MonitorStatus status = MonitorStatus.CREATED)
        {
            Symbol = symbol;
            Condition = cond;
            TriggerPrice = triggerPrice;
            MntrStatus = status;
            PostionId = positionId;
            CommandId = commandId;
        }
        public enum MonitorStatus
        {
            CREATED,
            ACTIVE,
            CLAIMED,
            CLOSED
        }
        public enum MonitorCondition
        {
            NEW_IS_HIGHER,
            NEW_IS_LOWER
        }
    }
}