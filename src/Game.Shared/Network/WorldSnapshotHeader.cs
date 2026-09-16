using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Game.Shared.Network
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct WorldSnapshotHeader
    {
        public const int Size = 18;

        public readonly ulong ServerTick;
        public readonly ulong ServerTimestampUs;
        public readonly ushort EntityCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WorldSnapshotHeader(ulong serverTick, ulong serverTimestampUs, ushort entityCount)
        {
            ServerTick = serverTick;
            ServerTimestampUs = serverTimestampUs;
            EntityCount = entityCount;
        }
    }
}
