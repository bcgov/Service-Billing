using System.ComponentModel.DataAnnotations;

namespace Service_Billing.Models
{
    public class Person
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        [Required]
        public string? DisplayName { get; set; }
        public string? Mail { get; set; }
    }
}
