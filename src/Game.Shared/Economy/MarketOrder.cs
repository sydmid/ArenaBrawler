using System;

namespace Game.Shared.Economy
{
    public class MarketOrder
    {
        public int OrderId { get; set; }
        public Guid PlayerId { get; set; }
        public string OrderType { get; set; } = string.Empty; // BUY, SELL
        public Guid? InstanceId { get; set; }
        public int? DefId { get; set; }
        public decimal TargetPrice { get; set; }
        public string Status { get; set; } = "OPEN"; // OPEN, FILLED, CANCELLED
        public DateTime CreatedAt { get; set; }
    }
}
