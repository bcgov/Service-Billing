namespace Service_Billing.Models.Repositories
{
    public interface IFiscalPeriodRepository
    {
        IEnumerable<FiscalPeriod> GetAll();
        FiscalPeriod? GetByFiscalQuarterString(string fiscalPeriod);
        FiscalPeriod? GetFiscalPeriodByString(string period);
        FiscalPeriod? GetFiscalPeriodById(int id);
        int SaveFiscalPeriod(FiscalPeriod fiscalPeriod);
    }
}
