using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Game.Shared.Models;
using Game.Shared.Network;

namespace Game.Client.Network
{
    public class GameClient : IDisposable
    {
        private readonly UdpClient _udpClient;
        private readonly IPEndPoint _serverEndPoint;
        private readonly CancellationTokenSource _cts = new();

        public ulong LocalPlayerId { get; }

        private readonly ConcurrentQueue<(WorldSnapshotHeader header, List<EntityState> states)> _snapshotQueue = new();

        public GameClient(string serverIp, int port, ulong playerId)
        {
            _serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), port);
            _udpClient = new UdpClient();
            _udpClient.Connect(_serverEndPoint);
            LocalPlayerId = playerId;
        }

        public void Start()
        {
            Task.Run(ReceiveLoop, _cts.Token);
        }

        public void SendInput(PlayerInputPayload payload)
        {
            Span<byte> buffer = stackalloc byte[PlayerInputPayload.BinarySize];
            payload.WriteTo(buffer);
            _udpClient.Send(buffer.ToArray(), PlayerInputPayload.BinarySize);
        }

        private async Task ReceiveLoop()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var result = await _udpClient.ReceiveAsync(_cts.Token);
                    var buffer = (ReadOnlySpan<byte>)result.Buffer;

                    if (buffer.Length >= WorldSnapshotHeader.Size)
                    {
                        var header = System.Runtime.InteropServices.MemoryMarshal.Read<WorldSnapshotHeader>(buffer.Slice(0, WorldSnapshotHeader.Size));

                        int expectedSize = WorldSnapshotHeader.Size + (header.EntityCount * EntityState.BinarySize);
                        if (buffer.Length == expectedSize)
                        {
                            var states = new List<EntityState>();
                            int offset = WorldSnapshotHeader.Size;
                            for (int i = 0; i < header.EntityCount; i++)
                            {
                                states.Add(EntityState.ReadFrom(buffer.Slice(offset, EntityState.BinarySize)));
                                offset += EntityState.BinarySize;
                            }

                            _snapshotQueue.Enqueue((header, states));
                        }
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GameClient] Receive error: {ex.Message}");
                }
            }
        }

        public bool TryGetLatestSnapshot(out (WorldSnapshotHeader header, List<EntityState> states) snapshot)
        {
            snapshot = default;
            bool found = false;
            while (_snapshotQueue.TryDequeue(out var currentSnapshot))
            {
                snapshot = currentSnapshot;
                found = true;
            }
            return found;
        }

        public void Dispose()
        {
            _cts.Cancel();
            _udpClient.Dispose();
        }
    }
}
