using System;
using System.Collections.Generic;
using HfEngine.Matching;
using HfEngine.Protocol;
using Xunit;

namespace Game.Server.Tests
{
    public class MockServerEgressSink : IEgressSink
    {
        public List<OrderAckPayload> Acks = new();
        public List<OrderExecutedPayload> Trades = new();
        public List<OrderBookL2DeltaPayload> Deltas = new();

        public void EmitAck(in OrderAckPayload ack) => Acks.Add(ack);
        public void EmitTrade(in OrderExecutedPayload trade) => Trades.Add(trade);
        public void EmitL2Delta(in OrderBookL2DeltaPayload delta) => Deltas.Add(delta);
    }

    public class HfStreamingIntegrationTests : IDisposable
    {
        private readonly OrderMemoryPool _pool;
        private readonly MockServerEgressSink _sink;
        private readonly OrderBook _book;

        public HfStreamingIntegrationTests()
        {
            _pool = new OrderMemoryPool();
            _sink = new MockServerEgressSink();
            _book = new OrderBook(101, _pool, _sink);
        }

        public void Dispose()
        {
            _pool.Dispose();
        }

        [Fact]
        public void HfEngine_OrderBook_InstantiatesAndProcessesLimitOrder()
        {
            Assert.Equal(101u, _book.AssetId);

            FrameHeader buyHeader = new(ProtocolConstants.MsgTypeNewOrder, ProtocolConstants.FlagSideBuy, 32);
            InboundFrame buyFrame = new(buyHeader, orderId: 1, traderId: 501, price: 15000, qty: 10, assetId: 101);

            _book.ProcessInboundFrame(in buyFrame);

            Assert.Equal(15000, _book.BestBidPrice);
            Assert.Single(_sink.Acks);
            Assert.Equal(ProtocolConstants.AckStatusAccepted, _sink.Acks[0].Status);
        }
    }
}
