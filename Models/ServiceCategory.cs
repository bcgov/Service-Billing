using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{
    public class ServiceCategory
    {
        [Key]
        [Column("Service Id")]
        public Int16 serviceId { get; set; }

        [Column("GDX Bus Area")]
        public string? businessArea { get; set; }

        [Column("Service Category")]
        public string? name { get; set; }

        [Column("Charge Type")]
        public string? chargeType { get; set; }

        [Column("Quantity")]
        public Int16? quantity { get; set; }

        [Column("Costs")]
        public string? costs { get; set; }

        [Column("Description")]
        public string? description { get; set; }

        public bool isActive { get; set; }

        public string? UOM { get; set; }
    }
}
