using System;
using System.Runtime.InteropServices;
using Game.Shared.Models;
using Xunit;

namespace Game.Shared.Tests
{
    public class EntityStateSerializationTests
    {
        [Fact]
        public void EntityState_HasExpectedBinarySize()
        {
            Assert.Equal(38, Marshal.SizeOf<EntityState>());
            Assert.Equal(38, EntityState.BinarySize);
        }

        [Fact]
        public void EntityState_RoundtripSerialization_MatchesOriginal()
        {
            EntityState original = new(
                entityId: 0xDEADBEEFCAFE,
                posX: 128.5f,
                posY: 256.75f,
                rotation: 3.14159f,
                velX: -15.2f,
                velY: 42.0f,
                mode: CombatMode.Ranged,
                flags: EntityStateFlags.Attacking | EntityStateFlags.Moving,
                health: 850,
                maxHealth: 1000,
                sequence: 9942
            );

            Span<byte> buffer = stackalloc byte[EntityState.BinarySize];
            original.WriteTo(buffer);

            EntityState reconstructed = EntityState.ReadFrom(buffer);

            Assert.Equal(original.EntityId, reconstructed.EntityId);
            Assert.Equal(original.PositionX, reconstructed.PositionX);
            Assert.Equal(original.PositionY, reconstructed.PositionY);
            Assert.Equal(original.Rotation, reconstructed.Rotation);
            Assert.Equal(original.VelocityX, reconstructed.VelocityX);
            Assert.Equal(original.VelocityY, reconstructed.VelocityY);
            Assert.Equal(original.CombatMode, reconstructed.CombatMode);
            Assert.Equal(original.StateFlags, reconstructed.StateFlags);
            Assert.Equal(original.Health, reconstructed.Health);
            Assert.Equal(original.MaxHealth, reconstructed.MaxHealth);
            Assert.Equal(original.Sequence, reconstructed.Sequence);
            Assert.Equal(original, reconstructed);
        }

        [Fact]
        public void EntityState_FlagManipulation_MaintainsBitmaskIntegrity()
        {
            EntityState state = new(1, 0, 0, 0, 0, 0, CombatMode.Melee, EntityStateFlags.None, 100, 100, 1);
            Assert.Equal(EntityStateFlags.None, state.StateFlags);

            state.StateFlags |= EntityStateFlags.Shielded;
            Assert.True((state.StateFlags & EntityStateFlags.Shielded) != 0);

            state.StateFlags |= EntityStateFlags.Stunned;
            Assert.True((state.StateFlags & EntityStateFlags.Stunned) != 0);
            Assert.True((state.StateFlags & EntityStateFlags.Shielded) != 0);

            state.StateFlags &= ~EntityStateFlags.Shielded;
            Assert.False((state.StateFlags & EntityStateFlags.Shielded) != 0);
            Assert.True((state.StateFlags & EntityStateFlags.Stunned) != 0);
        }
    }
}
