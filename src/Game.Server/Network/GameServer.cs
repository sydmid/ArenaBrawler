using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Game.Shared.Models;
using Game.Shared.Network;

namespace Game.Server.Network
{
    public class GameServer : IDisposable
    {
        private readonly UdpClient _udpClient;
        private readonly CancellationTokenSource _cts = new();
        private readonly ConcurrentDictionary<IPEndPoint, ulong> _clients = new();
        private readonly ConcurrentQueue<(IPEndPoint endPoint, PlayerInputPayload payload)> _inputQueue = new();

        private ulong _serverTick = 0;
        private readonly Stopwatch _stopwatch = new();

        // Entity state dictionary by player Id
        private readonly ConcurrentDictionary<ulong, EntityState> _entities = new();
        private readonly Game.Server.Economy.PersistenceChannel? _persistenceChannel;

        public GameServer(int port = 9050, Game.Server.Economy.PersistenceChannel? channel = null)
        {
            _udpClient = new UdpClient(port);
            _udpClient.Client.Blocking = false;
            _persistenceChannel = channel;
        }

        public void Start()
        {
            _stopwatch.Start();
            Task.Run(ReceiveLoop, _cts.Token);
            Task.Run(TickLoop, _cts.Token);
        }

        private async Task ReceiveLoop()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                try
                {
                    var result = await _udpClient.ReceiveAsync(_cts.Token);
                    if (result.Buffer.Length == PlayerInputPayload.BinarySize)
                    {
                        var payload = PlayerInputPayload.ReadFrom(result.Buffer);
                        _inputQueue.Enqueue((result.RemoteEndPoint, payload));

                        // Register new client
                        if (!_clients.ContainsKey(result.RemoteEndPoint))
                        {
                            _clients.TryAdd(result.RemoteEndPoint, payload.PlayerId);
                            if (!_entities.ContainsKey(payload.PlayerId))
                            {
                                _entities.TryAdd(payload.PlayerId, new EntityState(
                                    entityId: payload.PlayerId,
                                    posX: 640.0f,
                                    posY: 360.0f,
                                    rotation: 0.0f,
                                    velX: 0.0f,
                                    velY: 0.0f,
                                    mode: CombatMode.Melee,
                                    flags: EntityStateFlags.None,
                                    health: 1000,
                                    maxHealth: 1000,
                                    sequence: 0
                                ));
                            }
                        }
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    Console.WriteLine($"[GameServer] Receive error: {ex.Message}");
                }
            }
        }

        private async Task TickLoop()
        {
            // 60 Hz = ~16.666 ms per tick
            const double targetTickMs = 1000.0 / 60.0;
            long lastTickTime = _stopwatch.ElapsedMilliseconds;

            while (!_cts.Token.IsCancellationRequested)
            {
                long currentTickTime = _stopwatch.ElapsedMilliseconds;
                long deltaTimeMs = currentTickTime - lastTickTime;

                if (deltaTimeMs >= targetTickMs)
                {
                    _serverTick++;
                    ProcessInputs(deltaTimeMs / 1000.0f);
                    BroadcastSnapshot();

                    if (_persistenceChannel != null)
                    {
                        foreach (var state in _entities.Values)
                        {
                            _persistenceChannel.TryWrite(new Game.Server.Economy.PersistenceItem(in state));
                        }
                    }

                    lastTickTime = currentTickTime;
                }

                // Small delay to prevent tight loop maxing CPU
                await Task.Delay(1);
            }
        }

        private void ProcessInputs(float dt)
        {
            while (_inputQueue.TryDequeue(out var item))
            {
                var input = item.payload;
                if (_entities.TryGetValue(input.PlayerId, out var state))
                {
                    // Speed constant
                    float speed = 300.0f;

                    state.VelocityX = input.DirectionX * speed;
                    state.VelocityY = input.DirectionY * speed;

                    state.PositionX += state.VelocityX * dt;
                    state.PositionY += state.VelocityY * dt;

                    state.Rotation = input.AimAngle;

                    // Clamp to arena bounds (60, 60, 1280-60, 720-60)
                    float minX = 60;
                    float maxX = 1280 - 60;
                    float minY = 60;
                    float maxY = 720 - 60;

                    state.PositionX = Math.Clamp(state.PositionX, minX, maxX);
                    state.PositionY = Math.Clamp(state.PositionY, minY, maxY);

                    if (input.IsCombatToggleRequested)
                    {
                        state.CombatMode = state.CombatMode == CombatMode.Melee ? CombatMode.Ranged : CombatMode.Melee;
                    }

                    if (input.IsAttackRequested)
                    {
                        state.StateFlags |= EntityStateFlags.Attacking;
                    }
                    else
                    {
                        state.StateFlags &= ~EntityStateFlags.Attacking;
                    }

                    state.Sequence = input.InputSequence;

                    _entities[input.PlayerId] = state;
                }
            }
        }

        private void BroadcastSnapshot()
        {
            var header = new WorldSnapshotHeader(_serverTick, (ulong)(_stopwatch.ElapsedTicks * (1000000.0 / Stopwatch.Frequency)), (ushort)_entities.Count);

            // Build buffer
            int bufferSize = WorldSnapshotHeader.Size + (_entities.Count * EntityState.BinarySize);
            Span<byte> buffer = stackalloc byte[bufferSize];

            unsafe
            {
                fixed (byte* pBuffer = buffer)
                {
                    System.Runtime.InteropServices.MemoryMarshal.Write(buffer, in header);

                    int offset = WorldSnapshotHeader.Size;
                    foreach (var state in _entities.Values)
                    {
                        state.WriteTo(buffer.Slice(offset, EntityState.BinarySize));
                        offset += EntityState.BinarySize;
                    }
                }
            }

            foreach (var clientEp in _clients.Keys)
            {
                _udpClient.Send(buffer.ToArray(), bufferSize, clientEp);
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _udpClient.Dispose();
        }
    }
}
