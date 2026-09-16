using System;
using System.Runtime.InteropServices;
using Game.Shared.Models;
using Xunit;

namespace Game.Shared.Tests
{
    public class PlayerInputPayloadTests
    {
        [Fact]
        public void PlayerInputPayload_HasExpectedBinarySize()
        {
            Assert.Equal(34, Marshal.SizeOf<PlayerInputPayload>());
            Assert.Equal(34, PlayerInputPayload.BinarySize);
        }

        [Fact]
        public void PlayerInputPayload_RoundtripSerialization_MatchesOriginal()
        {
            PlayerInputPayload original = new(
                playerId: 777,
                inputSequence: 1042,
                dirX: 0.7071f,
                dirY: -0.7071f,
                aimAngle: 1.57f,
                attackRequested: true,
                combatToggleRequested: false,
                timestampUs: 1726000000000UL
            );

            Span<byte> buffer = stackalloc byte[PlayerInputPayload.BinarySize];
            original.WriteTo(buffer);

            PlayerInputPayload reconstructed = PlayerInputPayload.ReadFrom(buffer);

            Assert.Equal(original.PlayerId, reconstructed.PlayerId);
            Assert.Equal(original.InputSequence, reconstructed.InputSequence);
            Assert.Equal(original.DirectionX, reconstructed.DirectionX);
            Assert.Equal(original.DirectionY, reconstructed.DirectionY);
            Assert.Equal(original.AimAngle, reconstructed.AimAngle);
            Assert.True(reconstructed.IsAttackRequested);
            Assert.False(reconstructed.IsCombatToggleRequested);
            Assert.Equal(original.ClientTimestampUs, reconstructed.ClientTimestampUs);
        }
    }
}
