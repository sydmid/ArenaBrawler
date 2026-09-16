using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Game.Shared.Models;
using Game.Shared.Network;
using HfEngine.Protocol;
using HfEngine.Matching;
using Game.Server.Network;

namespace Game.Server
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("===========================================================");
            Console.WriteLine("   ARENA BRAWLER AUTHORITATIVE SERVER (.NET Core 9.0)      ");
            Console.WriteLine("===========================================================");

            using var marketplace = new Game.Server.Economy.MarketplaceEngine(101);
            marketplace.OnL2Delta += delta =>
            {
                Console.WriteLine($"[Server Egress] L2Delta: Price={delta.Price}, Qty={delta.NewQuantity}");
            };
            marketplace.OnTradeExecuted += trade =>
            {
                Console.WriteLine($"[Server Egress] Executed: TradeId={trade.TradeId}, Price={trade.ExecutionPrice}");
            };

            Console.WriteLine($"[Server] Linked MarketplaceEngine successfully.");

            // Verify Game.Shared payload schema contracts
            EntityState serverEntity = new(
                entityId: 1,
                posX: 100.0f,
                posY: 200.0f,
                rotation: 1.57f,
                velX: 0.0f,
                velY: 0.0f,
                mode: CombatMode.Melee,
                flags: EntityStateFlags.None,
                health: 1000,
                maxHealth: 1000,
                sequence: 1
            );

            byte[] stateBufferArray = new byte[EntityState.BinarySize];
            Span<byte> stateBuffer = stateBufferArray.AsSpan();
            serverEntity.WriteTo(stateBuffer);
            EntityState deserialized = EntityState.ReadFrom(stateBuffer);

            Debug.Assert(deserialized.EntityId == serverEntity.EntityId, "Entity state binary serialization validation failed");
            Console.WriteLine($"[Server] Verified unmanaged binary serialization. Payload size: {EntityState.BinarySize} bytes.");

            Console.WriteLine("[Server] Authoritative simulation loop initialized at 60 Hz tick target.");

            using var server = new GameServer(9050);
            server.Start();

            Console.WriteLine("[Server] Server running. Press Ctrl+C to exit.");

            var tcs = new TaskCompletionSource();
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                tcs.SetResult();
            };

            await tcs.Task;
            Console.WriteLine("[Server] Server shutting down.");
        }
    }
}
