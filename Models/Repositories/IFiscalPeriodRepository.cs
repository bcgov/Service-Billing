namespace Service_Billing.Models.Repositories
{
    public interface IFiscalPeriodRepository
    {
    //    void UpdateRecord(int chargeId, string fiscalPeriod, decimal? amount);
        FiscalPeriod? GetByFiscalQuarterString(string fiscalPeriod);
        int SaveFiscalPeriod(FiscalPeriod fiscalPeriod);
   //     IEnumerable<FiscalPeriod> GetPeriodsByChargeId(int chargeId);
    }
}
