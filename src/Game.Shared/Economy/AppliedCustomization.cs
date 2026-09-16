using System;

namespace Game.Shared.Economy
{
    public class AppliedCustomization
    {
        public int MappingId { get; set; }
        public Guid SkinInstanceId { get; set; }
        public int CustomizationDefId { get; set; }
        public int SlotIndex { get; set; } // 1 to 4
    }
}
