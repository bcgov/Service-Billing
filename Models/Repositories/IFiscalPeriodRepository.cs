namespace Service_Billing.Models.Repositories
{
    public interface IFiscalPeriodRepository
    {
        void UpdateRecord(int chargeId, string fiscalPeriod, decimal? amount);
        IEnumerable<FiscalPeriod> GetPeriodsByChargeId(int chargeId);
        Dictionary<int, decimal?> ChargeIdsAndCostByFiscalPeriod(string fiscalPeriod);
    }
}
