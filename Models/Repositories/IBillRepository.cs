using System.IO.Pipelines;

namespace Service_Billing.Models.Repositories
{
    public interface IBillRepository
    {
        IEnumerable<Bill> AllBills { get; }
        Bill? GetBill(int id);
        IEnumerable<Bill> SearchBillsByTitle(string searchQuery);
        IEnumerable<Bill> GetBillsByClientId(int  clientId);
        IEnumerable<Bill> GetBillsByServiceCategory(int serviceCategoryId);
        IEnumerable<Bill> GetBillsByAuthority(string expenseAuthority);
        IEnumerable<Bill> GetBillsByBillingCycle(DateOnly billingCycle);
        IEnumerable<Bill> GetBillsByDateRange(DateTime start, DateTime end);
        List<int> GetFixedServices();
        IEnumerable<Bill> GetCurrentQuarterBills();
        IEnumerable<Bill> GetPreviousQuarterBills();
        IEnumerable<Bill> GetNextQuarterBills();
        Task<int> CreateBill(Bill bill);
        Task PromoteChargesToNewQuarter();
        Task Update(Bill bill);
        Task UpdateAllChargesForServiceCategory(int serviceCategoryId);
        string GetPreviousQuarterString();
        string DetermineCurrentQuarter(DateTime? date = null);

        DateTime DetermineStartOfNextQuarter();

        Dictionary<int, decimal?> GetPreviousQuarterBillIds();
    }
}
