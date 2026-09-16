using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Game.Shared.Models;
using Game.Shared.Network;
using Game.Server.Network;

namespace Game.Server.Tests
{
    public class TickProgressionTests
    {
        [Fact]
        public async Task GameServer_ReceivesInput_And_BroadcastsSnapshot()
        {
            // Arrange
            int testPort = 9150;
            using var server = new GameServer(testPort);
            server.Start();

            using var testClient = new UdpClient();
            var serverEp = new IPEndPoint(IPAddress.Loopback, testPort);
            testClient.Connect(serverEp);

            var payload = new PlayerInputPayload(
                playerId: 99,
                inputSequence: 1,
                dirX: 1.0f,
                dirY: 0.0f,
                aimAngle: 0.0f,
                attackRequested: false,
                combatToggleRequested: false,
                timestampUs: 1000
            );

            Span<byte> inputBuffer = stackalloc byte[PlayerInputPayload.BinarySize];
            payload.WriteTo(inputBuffer);

            // Act
            // Send input to trigger entity creation and tick processing
            testClient.Send(inputBuffer.ToArray(), PlayerInputPayload.BinarySize);

            // Wait for a few ticks to pass
            await Task.Delay(100);

            // Try to receive a snapshot
            testClient.Client.ReceiveTimeout = 1000;
            IPEndPoint remoteEp = new IPEndPoint(IPAddress.Any, 0);
            byte[] recvBuffer = testClient.Receive(ref remoteEp);

            // Assert
            Assert.True(recvBuffer.Length >= WorldSnapshotHeader.Size);

            var header = System.Runtime.InteropServices.MemoryMarshal.Read<WorldSnapshotHeader>(recvBuffer.AsSpan().Slice(0, WorldSnapshotHeader.Size));
            Assert.True(header.ServerTick > 0);
            Assert.Equal(1, header.EntityCount);

            int expectedSize = WorldSnapshotHeader.Size + (header.EntityCount * EntityState.BinarySize);
            Assert.Equal(expectedSize, recvBuffer.Length);

            var state = EntityState.ReadFrom(recvBuffer.AsSpan().Slice(WorldSnapshotHeader.Size, EntityState.BinarySize));
            Assert.Equal(99ul, state.EntityId);

            // X should be > 640 because dirX = 1.0
            Assert.True(state.PositionX > 640.0f);
        }
    }
}
