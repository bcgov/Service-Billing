using System.ComponentModel.DataAnnotations;

namespace Service_Billing.Models
{

    public class ChangeLogEntry
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int EntityId { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy hh:mm:ss tt}", ApplyFormatInEditMode = true)]
        public DateTimeOffset DateModified { get; set; }
        [Required]
        public string ChangedBy { get; set; } = string.Empty;
        [Required]
        public string LogEntry { get; set; } = string.Empty;
        [Required]

        public string EntityType { get; set; } = string.Empty; // must be "charge", "clientAccount" or "service"

        public ChangeLogEntry() { }

        public ChangeLogEntry(int entityId, DateTimeOffset dateModified, string changedBy, string logEntry, string entityType)
        {
            EntityId = entityId;
            DateModified = dateModified;
            ChangedBy = changedBy;
            LogEntry = logEntry;
            EntityType = entityType;
        }
    }
}
