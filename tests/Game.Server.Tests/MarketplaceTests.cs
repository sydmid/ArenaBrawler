using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Game.Server.Economy;
using HfEngine.Protocol;

namespace Game.Server.Tests
{
    public class MarketplaceTests
    {
        [Fact]
        public void MarketplaceEngine_ProcessesBuyAndSell_GeneratesTrade()
        {
            // Arrange
            using var engine = new MarketplaceEngine(101);
            var trades = new List<OrderExecutedPayload>();

            engine.OnTradeExecuted += trade =>
            {
                trades.Add(trade);
            };

            // Act
            // Buyer places a limit order to buy 10 at price 100
            engine.PlaceBuyOrder(1001, 100, 10);

            // Seller places a market order to sell 5 at price 100
            engine.PlaceSellOrder(1002, 100, 5);

            // Assert
            Assert.Single(trades);
            Assert.Equal(100, trades[0].ExecutionPrice);
        }
    }
}
