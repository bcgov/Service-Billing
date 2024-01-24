using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{
    public class ServiceCategory
    {
        [Key]
        public int ServiceId { get; set; }

        [Required (ErrorMessage = "A business area must be defined")]
        [Display(Name = "GDX Business Area")]
        public string? GDXBusArea { get; set; }

        [Required(ErrorMessage = "Please give the service a descriptive name")]
        public string? Name { get; set; }

        [Display(Name = "Unit Price")]
        public string? Costs { get; set; }
        [BindRequired]
        public string? Description { get; set; }

        [Display(Name = "Is Active?")]
        public bool IsActive { get; set; }

        public string? UOM { get; set; }

        [Display(Name = "Service Owner")]
        public string? ServiceOwner { get; set; }

        [NotMapped]
        public bool UpdateCharges { get; set; } = false;
    }
}
