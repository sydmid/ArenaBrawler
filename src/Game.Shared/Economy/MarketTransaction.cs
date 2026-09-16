using System;

namespace Game.Shared.Economy
{
    public class MarketTransaction
    {
        public int TxId { get; set; }
        public Guid BuyerId { get; set; }
        public Guid SellerId { get; set; }
        public Guid InstanceId { get; set; }
        public decimal ExecutionPrice { get; set; }
        public DateTime ExecutedAt { get; set; }
    }
}
