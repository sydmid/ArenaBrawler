using System;

namespace Game.Shared.Economy
{
    public class InventoryItem
    {
        public Guid InstanceId { get; set; }
        public Guid PlayerId { get; set; }
        public int DefId { get; set; }
        public double WearFloat { get; set; }
        public bool IsTradeLocked { get; set; }
        public DateTime AcquiredAt { get; set; }
    }
}
