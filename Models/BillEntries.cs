using System.ComponentModel.DataAnnotations;

namespace Service_Billing.Models
{
    public class BillEntries
    {
        [Key]
        public int Id { get; set; }
        public string? Client_Account_Number_and_Name { get; set; }
        public string? Title { get; set; }
        public string? URL_or_IDIR { get; set; }
        public string? Service_Category { get; set; }
        public decimal Amount;
        public string? Fiscal_Period { get; set; }
        public decimal Unit_Price;
        public int Quantity { get; set; }
        public string? UOM { get; set; }
        public string? Aggregate_GL_Coding { get; set; }
        public string? Expense_Authority_Name { get; set; }
        public string? Ticket_Number_and_Requester_Name { get; set; }
        public DateTime Modified;
        public DateTime Created;
        public DateTime Billing_Cycle;
        public string? Created_By { get; set; }
        public string? Item_Type { get; set; }
    }
}
