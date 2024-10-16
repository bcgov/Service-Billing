
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Service_Billing.Data;

namespace Service_Billing.Models.Repositories
{
    public class FiscalHistoryRepository : IFiscalHistoryRepository
    {
        private readonly ServiceBillingContext _billingContext;
        private readonly ILogger<BillRepository> _logger;

        public FiscalHistoryRepository(ServiceBillingContext billingContext, ILogger<BillRepository> logger)
        {
            _billingContext = billingContext;
            _logger = logger;
        }
        public IEnumerable<FiscalHistory> All => _billingContext.FiscalHistory
            .Include(f => f.Bill);

        public IEnumerable<FiscalHistory> GetFiscalHistoriesByChargeId(int id)
        {
            return _billingContext.FiscalHistory.Where(x => x.BillId == id).Include(x => x.FiscalPeriod);
        }

        public IEnumerable<FiscalHistory> GetFiscalHistoryByFiscalPeriodId(int id)
        {
            return _billingContext.FiscalHistory.Where(x => x.PeriodId == id).Include(x => x.FiscalPeriod);
        }

        public FiscalHistory? GetFiscalHistoryById(int id)
        {
            return _billingContext.FiscalHistory.FirstOrDefault(x => x.Id == id);
        }

        public FiscalHistory? GetFiscalHistoryByIdAndChargeId(int id, int chargeId)
        {
            return _billingContext.FiscalHistory.FirstOrDefault(x => x.Id == id && x.BillId == chargeId);
        }

        public int SaveFiscalHistoryInfo(FiscalHistory fiscalHistory)
        {
            _billingContext.FiscalHistory.Add(fiscalHistory);
            _billingContext.SaveChanges();
            return fiscalHistory.Id;
        }

        public async Task UpdateFiscalHistory(FiscalHistory fiscalHistory) 
        {
            _billingContext.FiscalHistory.Update(fiscalHistory);
            await _billingContext.SaveChangesAsync();

        }
    }
}
