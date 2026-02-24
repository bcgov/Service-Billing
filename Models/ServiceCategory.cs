using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{
    public class ServiceCategory
    {
        [Key]
        public int ServiceId { get; set; }

        public int BusAreaId { get; set; }
        [ForeignKey("BusAreaId")]
        public virtual BusinessArea? BusArea { get; set; }
        [BindRequired]
        public string? Name { get; set; }

        [Display(Name = "Unit Price")]
        public string? Costs { get; set; }
        [BindRequired]
        public string? Description { get; set; }

        [Display(Name = "Is Active?")]
        public bool IsActive { get; set; }

        private string? _uom;
        public string? UOM
        {
            get => _uom == "Hr" ? "Hour" : _uom; // Return "Hour" if _uom is "Hr"
            set => _uom = value == "Hour" ? "Hr" : value; // 
        }
        [Display(Name = "Service Owner", Prompt = "Start typing in your contact's last name")]
        public string? ServiceOwner { get; set; }

        [NotMapped]
        public bool UpdateCharges { get; set; } = false;
    }
}
