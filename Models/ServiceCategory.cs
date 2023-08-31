using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{
    public class ServiceCategory
    {
        [Key]
        public int ServiceId { get; set; }

        public string? GDXBusArea { get; set; }

        public string? Name { get; set; }

        public string? Costs { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public string? UOM { get; set; }

        public string? ServiceOwner { get; set; }
    }
}
