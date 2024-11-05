
using DocumentFormat.OpenXml.Spreadsheet;
using Service_Billing.Data;

namespace Service_Billing.Models.Repositories
{
    public class ChangeLogRepository : IChangeLogRepository
    {
        private readonly ServiceBillingContext _billingContext;
        private readonly ILogger<BillRepository> _logger;

        public ChangeLogRepository(ServiceBillingContext billingContext, ILogger<BillRepository> logger)
        {
            _billingContext = billingContext;
            _logger = logger;
        }

        public async Task<int> CreateEntry(ChangeLogEntry entry)
        {
            await _billingContext.AddAsync(entry);
            await _billingContext.SaveChangesAsync();

            return entry.Id;
        }

        public IEnumerable<ChangeLogEntry> GetAll()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ChangeLogEntry> GetByEnityIdAndType(int entityId, string entityType)
        {
            return _billingContext.ChangeLogs.Where(x => x.EntityId == entityId && x.EntityType == entityType);
        }

        public ChangeLogEntry? GetByLogId(int id)
        {
            throw new NotImplementedException();
        }
    }
}
