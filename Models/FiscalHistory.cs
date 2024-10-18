using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Service_Billing.Models
{

    // Previous Fiscal History for a charge, so when a charge is promoted to a new quarter, we keep track of the 
    // last fiscal history, and what it the unit price was at that time. 
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
        public decimal? QuantityAtFiscal { get; set; }
        public string? Notes { get; set; }

        public FiscalHistory(int billId, int periodId, decimal? unitPriceAtFiscal, decimal? quantityAtFiscal, string? notes = null)
        {
            BillId = billId;
            PeriodId = periodId;
            UnitPriceAtFiscal = unitPriceAtFiscal;
            QuantityAtFiscal = quantityAtFiscal;
            Notes = notes;
        }

        public FiscalHistory() 
        {
            Id = 0;
            BillId = 0;
            PeriodId = 0;
            FiscalPeriod = null;
        }
    }
}
