using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Game.Shared.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EntityState : IEquatable<EntityState>
    {
        public const int BinarySize = 38;

        public ulong EntityId;
        public float PositionX;
        public float PositionY;
        public float Rotation;
        public float VelocityX;
        public float VelocityY;
        public byte Mode;             // Cast to CombatMode
        public byte Flags;            // Cast to EntityStateFlags
        public ushort Health;
        public ushort MaxHealth;
        public uint Sequence;

        public CombatMode CombatMode
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (CombatMode)Mode;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Mode = (byte)value;
        }

        public EntityStateFlags StateFlags
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (EntityStateFlags)Flags;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Flags = (byte)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EntityState(
            ulong entityId,
            float posX,
            float posY,
            float rotation,
            float velX,
            float velY,
            CombatMode mode,
            EntityStateFlags flags,
            ushort health,
            ushort maxHealth,
            uint sequence)
        {
            EntityId = entityId;
            PositionX = posX;
            PositionY = posY;
            Rotation = rotation;
            VelocityX = velX;
            VelocityY = velY;
            Mode = (byte)mode;
            Flags = (byte)flags;
            Health = health;
            MaxHealth = maxHealth;
            Sequence = sequence;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteTo(Span<byte> destination)
        {
            if (destination.Length < BinarySize)
                throw new ArgumentException($"Destination span must be at least {BinarySize} bytes.", nameof(destination));
            MemoryMarshal.Write(destination, in this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EntityState ReadFrom(ReadOnlySpan<byte> source)
        {
            if (source.Length < BinarySize)
                throw new ArgumentException($"Source span must be at least {BinarySize} bytes.", nameof(source));
            return MemoryMarshal.Read<EntityState>(source);
        }

        public bool Equals(EntityState other)
        {
            return EntityId == other.EntityId &&
                   PositionX.Equals(other.PositionX) &&
                   PositionY.Equals(other.PositionY) &&
                   Rotation.Equals(other.Rotation) &&
                   VelocityX.Equals(other.VelocityX) &&
                   VelocityY.Equals(other.VelocityY) &&
                   Mode == other.Mode &&
                   Flags == other.Flags &&
                   Health == other.Health &&
                   MaxHealth == other.MaxHealth &&
                   Sequence == other.Sequence;
        }

        public override bool Equals(object? obj) => obj is EntityState other && Equals(other);

        public override int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(EntityId);
            hash.Add(PositionX);
            hash.Add(PositionY);
            hash.Add(Rotation);
            hash.Add(VelocityX);
            hash.Add(VelocityY);
            hash.Add(Mode);
            hash.Add(Flags);
            hash.Add(Health);
            hash.Add(MaxHealth);
            hash.Add(Sequence);
            return hash.ToHashCode();
        }

        public static bool operator ==(EntityState left, EntityState right) => left.Equals(right);
        public static bool operator !=(EntityState left, EntityState right) => !left.Equals(right);
    }
}
