using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Game.Shared.Network
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct GameFrameHeader
    {
        public const int Size = 8;

        public readonly ushort Magic;
        public readonly byte MsgType;
        public readonly byte Flags;
        public readonly ushort PayloadLen;
        public readonly ushort Sequence;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GameFrameHeader(byte msgType, byte flags, ushort payloadLen, ushort sequence)
        {
            Magic = GameProtocolConstants.ArenaFrameMagic;
            MsgType = msgType;
            Flags = flags;
            PayloadLen = payloadLen;
            Sequence = sequence;
        }

        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Magic == GameProtocolConstants.ArenaFrameMagic;
        }
    }
}
