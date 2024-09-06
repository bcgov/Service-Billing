using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{
    public class Ministry
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "There must be a name set for the organization")]
        public string? Title { get; set; }
        [Required(ErrorMessage = "There must be an acronym or other short name set for the organization")]
        public string? Acronym { get; set; }
    }
}
