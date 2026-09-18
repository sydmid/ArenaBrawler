using System;
using System.Runtime.InteropServices;
using Game.Shared.Models;
using HfEngine.Protocol;

namespace Game.Server.Economy
{
    public enum PersistenceItemType : byte
    {
        None = 0,
        Trade = 1,
        StateSnapshot = 2
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct PersistenceItem
    {
        [FieldOffset(0)]
        public PersistenceItemType Type;

        [FieldOffset(1)]
        public OrderExecutedPayload Trade;

        [FieldOffset(1)]
        public EntityState State;

        public PersistenceItem(in OrderExecutedPayload trade)
        {
            this = default;
            Type = PersistenceItemType.Trade;
            Trade = trade;
        }

        public PersistenceItem(in EntityState state)
        {
            this = default;
            Type = PersistenceItemType.StateSnapshot;
            State = state;
        }
    }
}
