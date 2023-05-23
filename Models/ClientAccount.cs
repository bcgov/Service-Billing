using System.ComponentModel.DataAnnotations;

namespace Service_Billing.Models
{
    // see https://apps.itsm.gov.bc.ca/confluence/display/GDXSB/Account+Setup for client documentation
    public class ClientAccount
    {
        [Key]
        public int accountId { get; set; }
        public string? clientName { get; set; }
    }
}
