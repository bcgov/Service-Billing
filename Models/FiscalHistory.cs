using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{
    public class FiscalHistory
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int BillId { get; set; }
        [ForeignKey("BillId")]
        public virtual Bill? Bill { get; set; }
        [Required]
        public int PeriodId { get; set; }
        [ForeignKey("PeriodId")]
        public virtual FiscalPeriod? FiscalPeriod { get; set; }
        public decimal? UnitPriceAtFiscal { get; set; }
    }
}
