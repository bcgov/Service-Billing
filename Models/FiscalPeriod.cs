using System.ComponentModel.DataAnnotations;

namespace Service_Billing.Models
{
    public class FiscalPeriod
    {
        [Key]
        public int Id { get; set; }
        public string Period { get; set; }
    //    public virtual Bill Charge { get; set; } = null!; // "Required reference navigation to principal", says EF documentation
        public FiscalPeriod(string period)
        {
            Period = period;
        }
    }
}
