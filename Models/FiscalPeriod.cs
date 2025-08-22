using System.ComponentModel.DataAnnotations;

namespace Service_Billing.Models
{
    public class FiscalPeriod
    {
        [Key]
        public int Id { get; set; }
        public string Period { get; set; } = "No period set!";


        public FiscalPeriod(string period)
        {
            Period = period;
        }
        public FiscalPeriod()
        {
        }
    }
}
