using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Service_Billing.Models.Repositories
{
    public interface IChangeLogRepository
    {
        IEnumerable<ChangeLogEntry> GetAll();
        ChangeLogEntry? GetByLogId(int id);
        IEnumerable<ChangeLogEntry> GetByEnityIdAndType(int entityId, string entityType);

        Task<int> CreateEntry(ChangeLogEntry entry);

        Task<EntityEntry?> MakeChangeLogAndReturnEntry<T>(T entity, string userName);
    }
}
