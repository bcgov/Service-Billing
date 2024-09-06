namespace Service_Billing.Models.Repositories
{
    public interface IFiscalPeriodRepository
    {
        FiscalPeriod? GetByFiscalQuarterString(string fiscalPeriod);
        FiscalPeriod? GetFiscalPeriodByString(string period);
        FiscalPeriod? GetFiscalPeriodById(int id);
        int SaveFiscalPeriod(FiscalPeriod fiscalPeriod);
    }
}
