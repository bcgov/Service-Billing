using System.IO.Pipelines;

namespace Service_Billing.Models.Repositories
{
    public interface IBillRepositroy
    {
        IEnumerable<Bill> AllBills { get; }
        Bill? GetBill(int id);
        IEnumerable<Bill> SearchBillsByTitle(string searchQuery);
        IEnumerable<Bill> GetBillsByClientId(int  clientId);
        IEnumerable<Bill> GetBillsByServiceCategory(int serviceCategoryId);
        IEnumerable<Bill> GetBillsByAuthority(string expenseAuthority);
        IEnumerable<Bill> GetBillsByBillingCycle(DateOnly billingCycle);
        IEnumerable<Bill> GetBillsByDateRange(DateTime start, DateTime end);
        IEnumerable<Bill> GetCurrentQuarterBills();
        IEnumerable<Bill> GetPreviousQuarterBills();
        Task CreateBill(Bill bill);
        Task PromoteChargesToNewQuarter();
    }
}
