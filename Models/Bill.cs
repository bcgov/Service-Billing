using System.ComponentModel.DataAnnotations;

namespace Service_Billing.Models
{
    public class Bill
    {
        [Key]
        public int id { get; set; }
        public int clientAccountId { get; set; }
        public string? title { get; set; }
        public string? idirOrUrl { get; set; }
        public int serviceCategoryId { get; set; }
        public decimal amount;
        public string? fiscalPeriod { get; set; }
        //  public decimal Unit_Price; comes from service category lookup
        public int quantity { get; set; }
        public string? UOM { get; set; }
        public string? expenseAuthorityName { get; set; }
        public string? ticketNumberAndRequesterName { get; set; }
        public DateTime dateModified;
        public DateTime dateCreated;
        public DateTime billingCycle;
        public string? createdBy { get; set; }
        public string? itemType { get; set; }
    }
}
