using System;

namespace Game.Shared.Models
{
    [Flags]
    public enum EntityStateFlags : byte
    {
        None = 0,
        Attacking = 1 << 0,
        Stunned = 1 << 1,
        Shielded = 1 << 2,
        Moving = 1 << 3,
        Dead = 1 << 4
    }
}
