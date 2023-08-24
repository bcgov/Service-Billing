using System.ComponentModel.DataAnnotations;

namespace ServiceBilling.BillingManagement.UI.Models
{
    public class ServiceCategory
    {
        public Guid ServiceCategoryId { get; set; }
        
        [StringLength(100)]
        [Required]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "GDX Business Area")]
        [Required]
        public string GdxBusinessArea { get; set; } = string.Empty;
        public string Costs { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        
        public string UOM { get; set; } = string.Empty;

        public bool IsActive { get; set; } = false;
    }
}
