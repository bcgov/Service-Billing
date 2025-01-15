
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.DotNet.Scaffolding.Shared.Project;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Service_Billing.Data;
using System.Reflection;

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

        #region public interface
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

        public async Task<EntityEntry?> MakeChangeLogAndReturnEntry<T>(T entity, string userName)
        {
            try
            {
                DateTime utcDate = DateTime.UtcNow;
                TimeZoneInfo pacificZone = TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"); // Handles both PST and PDT
                DateTime pacificTime = TimeZoneInfo.ConvertTimeFromUtc(utcDate, pacificZone);

                if (entity == null)
                    throw new Exception("Null entity passed to MakeChangeLogEntry");

                Type modelType = entity.GetType();
                EntityEntry? entry = _billingContext.Entry(entity);
                string entityTypeString = GetEntityTypeString(modelType);
                string[] validTypes = { "charge", "clientAccount", "service" };

                if (!validTypes.Contains(entityTypeString))
                    throw new Exception($"Cannot make log entry for invalid entity type {modelType.Name}");
                int entityId = GetEntityId(modelType, entry);
                if (entityId == 0)
                    throw new Exception($"No entity Id was parsed for the changed entity of type {modelType.Name}");

                entry = MarkModifiedFields<T>(entity, entityId, entityTypeString, entry);
                _billingContext.ChangeTracker.DetectChanges();
                // Check if the property has changed
                string changes = string.Empty;
                if (entry?.State == EntityState.Modified)
                {
                    var modifiedProperties = entry.Properties
                        .Where(p => p.IsModified)
                        .Select(p => p.Metadata.Name);

                    foreach (var property in entry.Properties)
                    {
                        if (modifiedProperties.Contains(property.Metadata.Name) && property.OriginalValue != property.CurrentValue)
                        {
                            if (property.Metadata.Name == "ServiceCategoryId")
                            {
                                changes += GetServiceCategoryChangeString(property);
                            }
                            else
                                changes += $"{property.Metadata.Name} was changed from {property.OriginalValue} to {property.CurrentValue}</br>";
                        }
                    }
                    if (changes != String.Empty)
                        await _billingContext.ChangeLogs.AddAsync(new ChangeLogEntry(entityId, pacificTime, userName, changes, entityTypeString));
                }

                return entry;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error trying to create a change log entry for a charge. Exception message below.");
                _logger.LogError(ex.Message);
                return null;
            }
        }
        #endregion

        #region private methods

        private EntityEntry? MarkModifiedFields<T>(T currentEntity, int id, string typeString, EntityEntry? entry)
        {
            try
            {
                object? originalEntity = GetOriginalEntity<T>(id, typeString);
                if (originalEntity == null)
                    throw new Exception("Could not resolve original entity when trying to mark modified fields");
                PropertyInfo[]? properties = null;
                if (originalEntity is Bill)
                    properties = typeof(Bill).GetProperties();
                else if (originalEntity is ClientAccount)
                    properties = typeof(ClientAccount).GetProperties();
                else if (originalEntity is ServiceCategory)
                    properties = typeof(ServiceCategory).GetProperties();

                if (properties != null)
                    foreach (var property in properties)
                    {
                        if (property.Name == "DateModified")
                            continue;
                        // Check if the property can be written to and is not a navigation property
                        if (property.PropertyType != typeof(ServiceCategory)
                        && property.PropertyType != typeof(ClientAccount)
                        && property.PropertyType != typeof(FiscalPeriod)
                        && property.Name != "DateCreated"
                        && property.PropertyType != typeof(ICollection<FiscalHistory>)
                        && property.PropertyType != typeof(ICollection<Bill>)
                        && property.PropertyType != typeof(ICollection<Contact>)
                        && property.PropertyType != typeof(IEnumerable<Contact>)
                        )
                        {
                            var originalValue = property.GetValue(originalEntity);
                            var modifiedValue = property.GetValue(currentEntity);

                            // Check for differences
                            if (!Equals(originalValue, modifiedValue))
                            {
                                if (property.PropertyType == typeof(DateTimeOffset))
                                {
                                    DateTimeOffset originalDate;
                                    DateTimeOffset modifiedDate;
                                    if (DateTimeOffset.TryParse(property?.GetValue(originalEntity)?.ToString(), out originalDate)
                                    && DateTimeOffset.TryParse(property?.GetValue(originalEntity)?.ToString(), out modifiedDate))
                                    {
                                        if (Equals(originalDate.Date.ToShortDateString(), modifiedDate.Date.ToShortDateString())) // don't care if it's just a matter of hours
                                            continue;
                                    }
                                    else
                                    {
                                        throw new Exception("Could not parse dates from model");
                                    }
                                }

                                property.SetValue(originalEntity, modifiedValue);
                                _billingContext.Entry(originalEntity).Property(property.Name).IsModified = true;
                            }
                        }

                    }

                return _billingContext.Entry(originalEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred trying to mark modified fields for an {typeString} entity with id {id}");
                _logger.LogError(ex.Message, ex);
                return null;
            }
        }

        private object? GetOriginalEntity<T>(int id, string typeString)
        {
            switch (typeString)
            {
                case ("charge"):
                    return _billingContext.Bills.FirstOrDefault(x => x.Id == id);
                case ("clientAccount"):
                    return _billingContext.ClientAccounts.FirstOrDefault(x => x.Id == id);
                case ("service"):
                    return _billingContext.ServiceCategories.FirstOrDefault(x => x.ServiceId == id);
                default:
                    return null;
            }
        }

        private string GetEntityTypeString(Type modelType)
        {
            if (modelType == typeof(Bill)) // switch statements don't work with model types. That would be neat.
                return "charge";
            else if (modelType == typeof(ClientAccount))
                return "clientAccount";
            else if (modelType == typeof(ServiceCategory))
                return "service";
            else
                return $"Unsupported entity type {modelType.Name}";
        }

        private int GetEntityId(Type modelType, EntityEntry entry)
        {
            int entityId;
            if (modelType == typeof(Bill)) // switch statements don't work with model types. That would be neat.
            {
                if (!int.TryParse(entry.CurrentValues?["Id"]?.ToString(), out entityId))
                    throw new Exception("failed to parse an entity Id");
            }
            else if (modelType == typeof(ClientAccount))
            {
                if (!int.TryParse(entry.CurrentValues?["Id"]?.ToString(), out entityId))
                    throw new Exception("failed to parse an entity Id");
            }
            else if (modelType == typeof(ServiceCategory))
            {
                if (!int.TryParse(entry.CurrentValues?["ServiceId"]?.ToString(), out entityId))
                    throw new Exception("failed to parse an entity Id");
            }
            else
                throw new Exception($"Change log entries are not supported for entities of type {modelType.Name}");

            return entityId;
        }

        private string GetServiceCategoryChangeString(PropertyEntry property)
        {
            int originalServiceId;
            if (!int.TryParse(property?.OriginalValue?.ToString(), out originalServiceId))
                throw new Exception($"could not parse a service category Id from {property?.OriginalValue}");
            int currentServiceId;
            if (!int.TryParse(property?.OriginalValue?.ToString(), out currentServiceId))
                throw new Exception($"could not parse a service category Id from {property?.CurrentValue}");
            ServiceCategory? scOriginal = _billingContext.ServiceCategories.FirstOrDefault(x => x.ServiceId == originalServiceId);
            ServiceCategory? scCurrent = _billingContext.ServiceCategories.FirstOrDefault(x => x.ServiceId == currentServiceId);

            return $"The Service Category was changed from {scOriginal?.Name} to {scCurrent?.Name}</br>";
        }
    }

}

#endregion