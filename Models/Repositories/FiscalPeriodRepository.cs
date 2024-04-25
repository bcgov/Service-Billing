
using Service_Billing.Data;

namespace Service_Billing.Models.Repositories
{
    // For looking up bills that were active in previous quarters
    public class FiscalPeriodRepository : IFiscalPeriodRepository
    {
        private readonly ServiceBillingContext _billingContext;
        public FiscalPeriodRepository(ServiceBillingContext billingContext)
        {
            _billingContext = billingContext;
        }

        // note that records are updated here, but changes aren't saved to database. See IBillRepository.PromoteChargesToNewQuarter
          //  public async void UpdateRecord(int chargeId, string fiscalPeriod, decimal? amount)
          //  {
          //      FiscalPeriod record = new FiscalPeriod();
          ////      record.ChargeId = chargeId;
          //      record.Period = fiscalPeriod;
          ////      record.Amount = amount;
       
          //      await _billingContext.AddAsync(record);
          //  }

        public FiscalPeriod? GetByFiscalQuarterString(string fiscalPeriod)
        {
            return _billingContext.FiscalPeriods.FirstOrDefault(x => x.Period == fiscalPeriod);
        }

        public int SaveFiscalPeriod(FiscalPeriod fiscalPeriod)
        {
            _billingContext.FiscalPeriods.Add(fiscalPeriod);
            _billingContext.SaveChanges();
            return fiscalPeriod.Id;
        }

        //public IEnumerable<FiscalPeriod> GetPeriodsByChargeId(int chargeId)
        //{
        //   return _billingContext.FiscalPeriod.Where(p => p.ChargeId == chargeId);
        //}
    }
}
