using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Game.Shared.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PlayerInputPayload : IEquatable<PlayerInputPayload>
    {
        public const int BinarySize = 34;

        public ulong PlayerId;
        public uint InputSequence;
        public float DirectionX;
        public float DirectionY;
        public float AimAngle;
        public byte AttackRequested;
        public byte CombatToggleRequested;
        public ulong ClientTimestampUs;

        public bool IsAttackRequested
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => AttackRequested != 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => AttackRequested = (byte)(value ? 1 : 0);
        }

        public bool IsCombatToggleRequested
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => CombatToggleRequested != 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => CombatToggleRequested = (byte)(value ? 1 : 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public PlayerInputPayload(
            ulong playerId,
            uint inputSequence,
            float dirX,
            float dirY,
            float aimAngle,
            bool attackRequested,
            bool combatToggleRequested,
            ulong timestampUs)
        {
            PlayerId = playerId;
            InputSequence = inputSequence;
            DirectionX = dirX;
            DirectionY = dirY;
            AimAngle = aimAngle;
            AttackRequested = (byte)(attackRequested ? 1 : 0);
            CombatToggleRequested = (byte)(combatToggleRequested ? 1 : 0);
            ClientTimestampUs = timestampUs;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteTo(Span<byte> destination)
        {
            if (destination.Length < BinarySize)
                throw new ArgumentException($"Destination span must be at least {BinarySize} bytes.", nameof(destination));
            MemoryMarshal.Write(destination, in this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PlayerInputPayload ReadFrom(ReadOnlySpan<byte> source)
        {
            if (source.Length < BinarySize)
                throw new ArgumentException($"Source span must be at least {BinarySize} bytes.", nameof(source));
            return MemoryMarshal.Read<PlayerInputPayload>(source);
        }

        public bool Equals(PlayerInputPayload other)
        {
            return PlayerId == other.PlayerId &&
                   InputSequence == other.InputSequence &&
                   DirectionX.Equals(other.DirectionX) &&
                   DirectionY.Equals(other.DirectionY) &&
                   AimAngle.Equals(other.AimAngle) &&
                   AttackRequested == other.AttackRequested &&
                   CombatToggleRequested == other.CombatToggleRequested &&
                   ClientTimestampUs == other.ClientTimestampUs;
        }

        public override bool Equals(object? obj) => obj is PlayerInputPayload other && Equals(other);

        public override int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(PlayerId);
            hash.Add(InputSequence);
            hash.Add(DirectionX);
            hash.Add(DirectionY);
            hash.Add(AimAngle);
            hash.Add(AttackRequested);
            hash.Add(CombatToggleRequested);
            hash.Add(ClientTimestampUs);
            return hash.ToHashCode();
        }
    }
}
