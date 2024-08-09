
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

        public FiscalPeriod? GetByFiscalQuarterString(string fiscalPeriod)
        {
            return _billingContext.FiscalPeriods.FirstOrDefault(x => x.Period == fiscalPeriod);
        }

        public FiscalPeriod? GetFiscalPeriodByString(string period)
        {
            return _billingContext.FiscalPeriods.FirstOrDefault(x => x.Period == period);
        }

        public FiscalPeriod? GetFiscalPeriodById(int id)
        {
            return _billingContext.FiscalPeriods.FirstOrDefault(x => x.Id == id);
        }
        public int SaveFiscalPeriod(FiscalPeriod fiscalPeriod)
        {
            _billingContext.FiscalPeriods.Add(fiscalPeriod);
            _billingContext.SaveChanges();
            return fiscalPeriod.Id;
        }
    }
}
