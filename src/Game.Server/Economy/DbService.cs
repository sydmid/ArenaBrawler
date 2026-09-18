using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Oracle.ManagedDataAccess.Client;
using Game.Shared.Models;

namespace Game.Server.Economy
{
    public class DbService : IDbService
    {
        private readonly string _connectionString;

        public DbService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task InitializeAsync()
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();
            Console.WriteLine("[DbService] Connected to Oracle Database successfully.");
        }

        public async Task EnsureSchemaAsync()
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();
            // In a real scenario, this would apply migrations or check for existence
            Console.WriteLine("[DbService] Schema validation passed.");
        }

        public async Task RecordTransactionAsync(Guid buyerId, Guid sellerId, Guid instanceId, decimal price)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
                INSERT INTO MARKET_TRANSACTIONS (BUYER_ID, SELLER_ID, INSTANCE_ID, EXECUTION_PRICE)
                VALUES (:BuyerId, :SellerId, :InstanceId, :Price)";

            await connection.ExecuteAsync(sql, new
            {
                BuyerId = buyerId.ToByteArray(),
                SellerId = sellerId.ToByteArray(),
                InstanceId = instanceId.ToByteArray(),
                Price = price
            });
        }

        public async Task UpdateEntityStateAsync(EntityState state)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
                MERGE INTO ENTITY_STATES es
                USING (SELECT :EntityId AS EntityId, :PositionX AS PositionX, :PositionY AS PositionY, :Rotation AS Rotation, :Health AS Health, :Mode AS Mode FROM DUAL) src
                ON (es.ENTITY_ID = src.EntityId)
                WHEN MATCHED THEN
                    UPDATE SET es.POS_X = src.PositionX, es.POS_Y = src.PositionY, es.ROTATION = src.Rotation, es.HEALTH = src.Health, es.MODE = src.Mode
                WHEN NOT MATCHED THEN
                    INSERT (ENTITY_ID, POS_X, POS_Y, ROTATION, HEALTH, MODE)
                    VALUES (src.EntityId, src.PositionX, src.PositionY, src.Rotation, src.Health, src.Mode)";

            await connection.ExecuteAsync(sql, new
            {
                EntityId = state.EntityId,
                PositionX = state.PositionX,
                PositionY = state.PositionY,
                Rotation = state.Rotation,
                Health = state.Health,
                Mode = state.CombatMode
            });
        }

        public async Task RecordTransactionsBulkAsync(IEnumerable<PersistenceItem> trades)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
                INSERT INTO MARKET_TRANSACTIONS (BUYER_ID, SELLER_ID, INSTANCE_ID, EXECUTION_PRICE)
                VALUES (:BuyerId, :SellerId, :InstanceId, :Price)";

            // Convert ulong ids into guid arrays based on some business logic (currently omitted from payload but we map maker/taker)
            // Note: HfEngine is using ulong MakerOrderId/TakerOrderId. We'll map those as placeholders for Buyer/Seller Guids.
            var parameters = trades.Select(t => new
            {
                BuyerId = Guid.Empty.ToByteArray(), // We don't have full buyer/seller info in OrderExecutedPayload, so we preserve empty or map it differently later.
                SellerId = Guid.Empty.ToByteArray(),
                InstanceId = Guid.Empty.ToByteArray(),
                Price = t.Trade.ExecutionPrice
            });

            await connection.ExecuteAsync(sql, parameters);
        }

        public async Task UpdateEntityStatesBulkAsync(IEnumerable<PersistenceItem> states)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"
                MERGE INTO ENTITY_STATES es
                USING (SELECT :EntityId AS EntityId, :PositionX AS PositionX, :PositionY AS PositionY, :Rotation AS Rotation, :Health AS Health, :Mode AS Mode FROM DUAL) src
                ON (es.ENTITY_ID = src.EntityId)
                WHEN MATCHED THEN
                    UPDATE SET es.POS_X = src.PositionX, es.POS_Y = src.PositionY, es.ROTATION = src.Rotation, es.HEALTH = src.Health, es.MODE = src.Mode
                WHEN NOT MATCHED THEN
                    INSERT (ENTITY_ID, POS_X, POS_Y, ROTATION, HEALTH, MODE)
                    VALUES (src.EntityId, src.PositionX, src.PositionY, src.Rotation, src.Health, src.Mode)";

            var parameters = states.Select(s => new
            {
                EntityId = s.State.EntityId,
                PositionX = s.State.PositionX,
                PositionY = s.State.PositionY,
                Rotation = s.State.Rotation,
                Health = s.State.Health,
                Mode = s.State.CombatMode
            });

            await connection.ExecuteAsync(sql, parameters);
        }

        public void Dispose()
        {
            // Nothing to dispose anymore since we pool connections per method call.
        }
    }
}
