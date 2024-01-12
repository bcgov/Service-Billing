namespace Service_Billing.Models
{
    public class FiscalPeriod
    {
        public int Id { get; set; }
        public int ChargeId { get; set; }
        public string Period { get; set; }
        public decimal? Amount { get; set; }
        public virtual Bill Charge { get; set; } = null!; // "Required reference navigation to principal", says EF documentation
    }
}
