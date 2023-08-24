using System.ComponentModel.DataAnnotations;

namespace ServiceBilling.BillingManagement.UI.Models
{
    public class ServiceCategory
    {
        public Guid ServiceCategoryId { get; set; }
        
        [StringLength(100)]
        [Required(ErrorMessage = "Please enter a name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "GDX Business Area")]
        [Required(ErrorMessage = "Please enter a business area")]
        public string GdxBusinessArea { get; set; } = string.Empty;
        public string Costs { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description should not be longer than 1000 characters")]
        public string Description { get; set; } = string.Empty;
        
        public string UOM { get; set; } = string.Empty;
    }
}
