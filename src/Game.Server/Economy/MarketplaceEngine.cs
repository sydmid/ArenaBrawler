using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Game.Shared.Economy;
using HfEngine.Matching;
using HfEngine.Protocol;

namespace Game.Server.Economy
{
    public class MarketplaceEngine : IDisposable
    {
        private readonly OrderMemoryPool _pool;
        private readonly OrderBook _orderBook;
        private ulong _orderIdCounter = 1;

        public event Action<OrderExecutedPayload>? OnTradeExecuted;
        public event Action<OrderBookL2DeltaPayload>? OnL2Delta;

        private class InternalEgressSink : IEgressSink
        {
            private readonly MarketplaceEngine _engine;
            public InternalEgressSink(MarketplaceEngine engine) => _engine = engine;

            public void EmitAck(in OrderAckPayload ack) { }

            public void EmitTrade(in OrderExecutedPayload trade)
            {
                _engine.OnTradeExecuted?.Invoke(trade);
            }

            public void EmitL2Delta(in OrderBookL2DeltaPayload delta)
            {
                _engine.OnL2Delta?.Invoke(delta);
            }
        }

        private readonly TransactionQueue _txQueue = new();

        public MarketplaceEngine(uint assetId)
        {
            _pool = new OrderMemoryPool();
            var sink = new InternalEgressSink(this);
            _orderBook = new OrderBook(assetId, _pool, sink);

            OnTradeExecuted += _txQueue.EnqueueTrade;
            _txQueue.Start();
        }

        public void PlaceBuyOrder(ulong traderId, long price, uint quantity)
        {
            var header = new FrameHeader(ProtocolConstants.MsgTypeNewOrder, ProtocolConstants.FlagSideBuy, 32);
            var frame = new InboundFrame(header, Interlocked.Increment(ref _orderIdCounter), traderId, price, quantity, _orderBook.AssetId);
            _orderBook.ProcessInboundFrame(in frame);
        }

        public void PlaceSellOrder(ulong traderId, long price, uint quantity)
        {
            var header = new FrameHeader(ProtocolConstants.MsgTypeNewOrder, ProtocolConstants.FlagSideSell, 32);
            var frame = new InboundFrame(header, Interlocked.Increment(ref _orderIdCounter), traderId, price, quantity, _orderBook.AssetId);
            _orderBook.ProcessInboundFrame(in frame);
        }

        public void Dispose()
        {
            _txQueue.Dispose();
            _pool.Dispose();
        }
    }
}
