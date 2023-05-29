using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{
    public class ServiceCategory
    {
        [Key]
        [Column("Service_Id")]
        public int serviceId { get; set; }
        [Column("GDX_Bus_Area")]
        public string? businessArea { get; set; }
        [Column("Service_Category")]
        public string? name { get; set; }
        [Column("Costs")]
        public string? costs { get; set; }
        [Column("Details_Description")]
        public string? description { get; set; }
    }
}
