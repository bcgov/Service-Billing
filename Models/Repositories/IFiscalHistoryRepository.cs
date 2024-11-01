namespace Service_Billing.Models.Repositories
{
    public interface IFiscalHistoryRepository
    {
        IEnumerable<FiscalHistory> All { get; }
        FiscalHistory? GetFiscalHistoryById(int id);
        IEnumerable<FiscalHistory> GetFiscalHistoriesByChargeId(int id);
        IEnumerable<FiscalHistory> GetFiscalHistoryByFiscalPeriodId(int id);
        FiscalHistory? GetFiscalHistoryByIdAndChargeId(int id, int chargeId);
        int SaveFiscalHistoryInfo(FiscalHistory fiscalHistory);
        Task UpdateFiscalHistory(FiscalHistory fiscalHistory);
    }
}
