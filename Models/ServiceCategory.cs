using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{
    public class ServiceCategory
    {
        [Key]
        public int ServiceId { get; set; }

        //[Required (ErrorMessage = "Select the Government Digital Experience (GDX) business area from the drop down.")]
        //[Display(Name = "GDX Business Area")]
        //public string? GDXBusArea { get; set; }

        //[Required(ErrorMessage = "Add the name of the digital asset / web property / IDIR. Don't use acronyms.")]

        public int BusAreaId { get; set; }
        [ForeignKey("BusAreaId")]
        public virtual BusinessArea BusinessArea { get; set; }
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
