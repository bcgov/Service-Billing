
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
        public async void UpdateRecord(int chargeId, string fiscalPeriod)
        {
            FiscalPeriod record = new FiscalPeriod();
            record.ChargeId = chargeId;
            record.Period = fiscalPeriod;
       
            await _billingContext.AddAsync(record);
        }

        public List<int> ChargeIdsByFiscalPeriod(string fiscalPeriod)
        {
            return _billingContext.FiscalPeriod.Where( p => p.Period == fiscalPeriod).Select(p => p.ChargeId).ToList();
        }

        public IEnumerable<FiscalPeriod> GetPeriodsByChargeId(int chargeId)
        {
           return _billingContext.FiscalPeriod.Where(p => p.ChargeId == chargeId);
        }
    }
}
