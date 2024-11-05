namespace Service_Billing.Models.Repositories
{
    public interface IChangeLogRepository
    {
        IEnumerable<ChangeLogEntry> GetAll();
        ChangeLogEntry? GetByLogId(int id);
        IEnumerable<ChangeLogEntry> GetByEnityIdAndType(int entityId, string entityType);

        Task<int> CreateEntry(ChangeLogEntry entry);
    }
}
