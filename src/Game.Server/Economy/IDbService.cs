using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Shared.Models;

namespace Game.Server.Economy
{
    public interface IDbService : IDisposable
    {
        Task InitializeAsync();
        Task EnsureSchemaAsync();
        Task RecordTransactionAsync(Guid buyerId, Guid sellerId, Guid instanceId, decimal price);
        Task UpdateEntityStateAsync(EntityState state);
        Task RecordTransactionsBulkAsync(IEnumerable<PersistenceItem> trades);
        Task UpdateEntityStatesBulkAsync(IEnumerable<PersistenceItem> states);
    }
}
