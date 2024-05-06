namespace Service_Billing.Models.Repositories
{
    public interface IFiscalPeriodRepository
    {
        FiscalPeriod? GetByFiscalQuarterString(string fiscalPeriod);
        FiscalPeriod? GetFiscalPeriodByString(string period);
        int SaveFiscalPeriod(FiscalPeriod fiscalPeriod);
    }
}
