namespace Service_Billing.Models.Repositories
{
    public interface IFiscalPeriodRepository
    {
        void UpdateRecord(int chargeId, string fiscalPeriod);
        IEnumerable<FiscalPeriod> GetPeriodsByChargeId(int chargeId);
        List<int> ChargeIdsByFiscalPeriod(string fiscalPeriod);
    }
}
