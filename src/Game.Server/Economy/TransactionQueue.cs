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
        private readonly PersistenceChannel _channel;

        public TransactionQueue(PersistenceChannel channel)
        {
            _channel = channel;
        }

        public void EnqueueTrade(OrderExecutedPayload trade)
        {
            _channel.TryWrite(new PersistenceItem(in trade));
        }

        public void Start()
        {
            // Background processing is now handled by DatabaseWorkerService
        }

        public void Dispose()
        {
            // Resources are managed elsewhere
        }
    }
}
