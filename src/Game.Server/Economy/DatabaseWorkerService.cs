using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Game.Server.Economy
{
    public class DatabaseWorkerService : BackgroundService
    {
        private readonly IDbService _dbService;
        private readonly PersistenceChannel _channel;
        private const int BatchSize = 100;
        private readonly TimeSpan _batchTimeout = TimeSpan.FromMilliseconds(50);

        public DatabaseWorkerService(IDbService dbService, PersistenceChannel channel)
        {
            _dbService = dbService;
            _channel = channel;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("[DatabaseWorkerService] Starting background persistence consumer.");
            var batch = new List<PersistenceItem>(BatchSize);
            var trades = new List<PersistenceItem>(BatchSize);
            var states = new List<PersistenceItem>(BatchSize);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                    cts.CancelAfter(_batchTimeout);

                    try
                    {
                        await foreach (var item in _channel.Reader.ReadAllAsync(cts.Token))
                        {
                            batch.Add(item);
                            if (batch.Count >= BatchSize)
                            {
                                break;
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when batch timeout occurs or main token cancels
                    }

                    if (batch.Count > 0)
                    {
                        await ProcessBatchAsync(batch, trades, states);
                        batch.Clear();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[DatabaseWorkerService] Cancellation requested. Draining channel...");
            }
            finally
            {
                _channel.Complete();

                // Final drain
                while (_channel.Reader.TryRead(out var item))
                {
                    batch.Add(item);
                }

                if (batch.Count > 0)
                {
                    await ProcessBatchAsync(batch, trades, states);
                    batch.Clear();
                }

                Console.WriteLine("[DatabaseWorkerService] Persistence channel drained and stopped.");
            }
        }

        private async Task ProcessBatchAsync(List<PersistenceItem> batch, List<PersistenceItem> trades, List<PersistenceItem> states)
        {
            trades.Clear();
            states.Clear();

            foreach (var item in batch)
            {
                if (item.Type == PersistenceItemType.Trade) trades.Add(item);
                else if (item.Type == PersistenceItemType.StateSnapshot) states.Add(item);
            }

            try
            {
                if (trades.Count > 0)
                {
                    await _dbService.RecordTransactionsBulkAsync(trades);
                }

                if (states.Count > 0)
                {
                    await _dbService.UpdateEntityStatesBulkAsync(states);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DatabaseWorkerService] Error processing batch: {ex.Message}");
            }
        }
    }
}
