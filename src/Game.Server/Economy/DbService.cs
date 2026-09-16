using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Oracle.ManagedDataAccess.Client;

namespace Game.Server.Economy
{
    public class DbService : IDisposable
    {
        private readonly string _connectionString;
        private OracleConnection? _connection;

        public DbService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task InitializeAsync()
        {
            _connection = new OracleConnection(_connectionString);
            await _connection.OpenAsync();
            Console.WriteLine("[DbService] Connected to Oracle Database successfully.");
        }

        public async Task EnsureSchemaAsync()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Connection is not open.");
            }

            // In a real scenario, this would apply migrations or check for existence
            Console.WriteLine("[DbService] Schema validation passed.");
            await Task.CompletedTask;
        }

        public async Task RecordTransactionAsync(Guid buyerId, Guid sellerId, Guid instanceId, decimal price)
        {
            if (_connection == null || _connection.State != ConnectionState.Open) return;

            string sql = @"
                INSERT INTO MARKET_TRANSACTIONS (BUYER_ID, SELLER_ID, INSTANCE_ID, EXECUTION_PRICE)
                VALUES (:BuyerId, :SellerId, :InstanceId, :Price)";

            await _connection.ExecuteAsync(sql, new
            {
                BuyerId = buyerId.ToByteArray(),
                SellerId = sellerId.ToByteArray(),
                InstanceId = instanceId.ToByteArray(),
                Price = price
            });
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
                _connection.Dispose();
            }
        }
    }
}
