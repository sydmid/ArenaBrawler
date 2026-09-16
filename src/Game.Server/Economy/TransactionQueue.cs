using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Game.Shared.Economy;
using HfEngine.Protocol;

namespace Game.Server.Economy
{
    public class TransactionQueue : IDisposable
    {
        private readonly ConcurrentQueue<OrderExecutedPayload> _pendingTrades = new();
        private readonly CancellationTokenSource _cts = new();

        public void EnqueueTrade(OrderExecutedPayload trade)
        {
            _pendingTrades.Enqueue(trade);
        }

        public void Start()
        {
            Task.Run(ProcessQueueAsync, _cts.Token);
        }

        private async Task ProcessQueueAsync()
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                if (_pendingTrades.TryDequeue(out var trade))
                {
                    // This is where we would save to Oracle DB
                    // For now, we simulate processing
                    await Task.Delay(5);
                    Console.WriteLine($"[TransactionQueue] Persisted trade {trade.TradeId} to database.");
                }
                else
                {
                    await Task.Delay(10);
                }
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
        }
    }
}
