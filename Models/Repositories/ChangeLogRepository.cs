
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
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

        public async Task MakeChangeLogEntry(object entity, string userName)
        {
            try
            {
                DateTime utcDate = DateTime.UtcNow;
                TimeZoneInfo pacificZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"); // Handles both PST and PDT
                DateTime pacificTime = TimeZoneInfo.ConvertTimeFromUtc(utcDate, pacificZone);

                if (entity == null)
                    throw new Exception("Null entity passed to MakeChangeLogEntry");

                Type modelType = entity.GetType();
                var entry = _billingContext.Entry(entity);
                string entityTypeString = string.Empty;
                int entityId = 0;
                if (modelType == typeof(Bill)) // switch statements don't work with model types. That would be neat.
                {
                    if (!int.TryParse(entry.CurrentValues?["Id"]?.ToString(), out entityId))
                        throw new Exception("failed to parse an entity Id");
                    entityTypeString = "charge";
                }
                else if (modelType == typeof(ClientAccount))
                {
                    if (!int.TryParse(entry.CurrentValues?["Id"]?.ToString(), out entityId))
                        throw new Exception("failed to parse an entity Id");
                    entityTypeString = "clientAccount";
                }
                else if (modelType == typeof(ServiceCategory))
                {
                    if (!int.TryParse(entry.CurrentValues?["ServiceId"]?.ToString(), out entityId))
                        throw new Exception("failed to parse an entity Id");
                    entityTypeString = "service";
                }
                else
                    throw new Exception($"Change log entries are not supported for entities of type {modelType.Name}");
                if (entityId == 0)
                    throw new Exception($"No entity Id was parsed for the changed entity of type {modelType.Name}");
                // Get all properties of the model

                // Detect changes
                _billingContext.ChangeTracker.DetectChanges();

                // Check if the property has changed
                string changes = string.Empty;
                if (entry.State == EntityState.Modified)
                {
                    var modifiedProperties = entry.Properties
                        .Where(p => p.IsModified)
                        .Select(p => p.Metadata.Name);

                    foreach (var property in entry.Properties)
                    {
                        if (modifiedProperties.Contains(property.Metadata.Name) && property.OriginalValue != property.CurrentValue)
                            changes += $"{property.Metadata.Name} was changed from {property.OriginalValue} to {property.CurrentValue} \n";
                    }
                    if (changes != String.Empty)
                        await _billingContext.ChangeLogs.AddAsync(new ChangeLogEntry(entityId, pacificTime, userName, changes, entityTypeString));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error trying to create a change log entry for a charge. Exception message below.");
                _logger.LogError(ex.Message);
            }
        }
    }
}
