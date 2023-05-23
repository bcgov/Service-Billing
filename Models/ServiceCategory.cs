using System.ComponentModel.DataAnnotations;

namespace Service_Billing.Models
{
    public class ServiceCategory
    {
        [Key]
        public int serviceId { get; set; }
        public string? name { get; set; }
        public decimal? uintPrice { get; set; }
    }
}
