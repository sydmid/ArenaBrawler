using System;

namespace Game.Shared.Economy
{
    public class ItemDefinition
    {
        public int DefId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // WEAPON_SKIN, STICKER, CHARM, CASE
        public string Rarity { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
    }
}
