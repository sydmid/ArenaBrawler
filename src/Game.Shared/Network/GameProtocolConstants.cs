namespace Game.Shared.Network
{
    public static class GameProtocolConstants
    {
        public const ushort ArenaFrameMagic = 0x4142; // 'A', 'B'

        // Core Arena Gameplay Message IDs
        public const byte MsgTypePlayerInput = 0x10;
        public const byte MsgTypeEntityState = 0x11;
        public const byte MsgTypeWorldSnapshot = 0x12;
        public const byte MsgTypeCombatModeChange = 0x13;

        // High-Frequency Market Integration Message IDs (compatible with hf-state-streaming)
        public const byte MsgTypeMarketNewOrder = 0x01;
        public const byte MsgTypeMarketCancelOrder = 0x02;
        public const byte MsgTypeMarketOrderAck = 0x03;
        public const byte MsgTypeMarketOrderExecuted = 0x04;
        public const byte MsgTypeMarketL2Delta = 0x05;

        public const int HeaderSize = 8;
        public const int MaxPacketSize = 1400; // MTU safe bound for UDP / IPC streaming
    }
}
