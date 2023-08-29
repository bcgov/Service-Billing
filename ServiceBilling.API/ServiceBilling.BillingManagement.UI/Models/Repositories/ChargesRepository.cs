using ServiceBilling.BillingManagement.UI.Models;
using ServiceBilling.BillingManagement.UI.Models.Repositories;

namespace ServiceBilling.BillingManagement.UI.Models.Repositories
{
    public class ChargesRepository : IChargesRepository
    {

        private readonly DataContext _dataContext;
        public ChargesRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public IEnumerable<Charge> AllCharges => _dataContext.Charges.OrderBy(b => b.Title);

        public string DetermineCurrentQuarter(DateTime? date = null)
        {
            DateTime today = DateTime.Today;
            if (date != null)
                today = date.Value;
            string quarter = "";
            string year1 = today.Year.ToString();
            string year2 = (today.Year + 1).ToString();

            switch (today.Month)
            {
                case 4:
                case 5:
                case 6:
                    quarter = "Quarter 1";
                    break;
                case 7:
                case 8:
                case 9:
                    quarter = "Quarter 2";
                    break;
                case 10:
                case 11:
                case 12:
                    quarter = "Quarter 3";
                    break;
                case 1:
                case 2:
                case 3:
                    quarter = "Quarter 4";
                    break;
            }

            return $"Fiscal {year1.Substring(2)}/{year2.Substring(2)} {quarter}";
        }

        public IEnumerable<Charge> GetCurrentQuarterCharges()
        {
            string fiscalPeriod = DetermineCurrentQuarter();
            return _dataContext.Charges.Where(b => b.FiscalPeriod == fiscalPeriod);
        }

        public IEnumerable<Charge> GetPreviousQuarterCharges()
        {
            try
            {
                string currentFiscalPeriod = DetermineCurrentQuarter();
                //we need to go to quarter 4 of last year
                if (currentFiscalPeriod.Contains("Quarter 1"))
                { //Fiscal 23/24 Quarter 1
                    int year;
                    var x = currentFiscalPeriod.Substring(10, 2);
                    if (!int.TryParse(currentFiscalPeriod.Substring(10, 2), null, out year))
                    {
                        throw new Exception("Could not parse year from Fiscal Period string");
                    }
                    return _dataContext.Charges.Where(b => b.FiscalPeriod == $"Fiscal {year - 2}/{year - 1} Quarter 4");
                }
                else
                {
                    int quarter;
                    if (!int.TryParse(currentFiscalPeriod.Last().ToString(), null, out quarter))
                    {
                        throw new Exception("Could not parse Quarter from Fiscal Period string");
                    }
                    currentFiscalPeriod = currentFiscalPeriod.Remove(currentFiscalPeriod.Length - 1);
                    currentFiscalPeriod += (quarter - 1);
                    return _dataContext.Charges.Where(b => b.FiscalPeriod == currentFiscalPeriod);
                }
            }
            catch (Exception e)
            {
                //Todo: Add error logging.
            }

            return null;
        }

        public Charge? GetCharge(int id)
        {
            return _dataContext.Charges.FirstOrDefault(b => b.Id == id);
        }

        public IEnumerable<Charge> SearchChargesByTitle(string searchQuery)
        {
            return _dataContext.Charges.Where(b => !string.IsNullOrEmpty(b.Title) && b.Title.Contains(searchQuery));
        }

        public async Task CreateCharge(Charge Charge)
        {
            await _dataContext.AddAsync(Charge);
            await _dataContext.SaveChangesAsync();
        }

    }
}
