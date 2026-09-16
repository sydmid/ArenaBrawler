using System;
using Game.Shared.Models;
using Game.Shared.Network;
using Xunit;

namespace Game.Client.Tests
{
    public class ClientInitializationTests
    {
        [Fact]
        public void LocalPlayer_InitializesWithDefaultMeleeState()
        {
            EntityState player = new(
                entityId: 1001,
                posX: 640f,
                posY: 360f,
                rotation: 0f,
                velX: 0f,
                velY: 0f,
                mode: CombatMode.Melee,
                flags: EntityStateFlags.None,
                health: 1000,
                maxHealth: 1000,
                sequence: 1
            );

            Assert.Equal(CombatMode.Melee, player.CombatMode);
            Assert.Equal(1000, player.Health);
            Assert.Equal(640f, player.PositionX);
            Assert.Equal(360f, player.PositionY);
            Assert.False((player.StateFlags & EntityStateFlags.Attacking) != 0);
        }

        [Theory]
        [InlineData(1f, 0f, 250f, 0.016f, 4f, 0f)]
        [InlineData(0f, -1f, 250f, 0.016f, 0f, -4f)]
        public void MovementVector_CalculatesExpectedDisplacement(
            float dirX, float dirY, float speed, float deltaTime, float expectedDeltaX, float expectedDeltaY)
        {
            float deltaX = dirX * speed * deltaTime;
            float deltaY = dirY * speed * deltaTime;

            Assert.Equal(expectedDeltaX, deltaX, 3);
            Assert.Equal(expectedDeltaY, deltaY, 3);
        }
    }
}
