using ServiceBilling.BillingManagement.UI.Models;

namespace ServiceChargeing.ChargeingManagement.UI.Models.Repositories
{
    public interface IChargesRepository
    {
        IEnumerable<Charge> AllCharges { get; }
        Charge? GetCharge(int id);
        IEnumerable<Charge> SearchChargesByTitle(string searchQuery);
        //IEnumerable<Charge> GetChargesByClientId(int clientId);
       // IEnumerable<Charge> GetChargesByServiceCategory(int serviceCategoryId);
        IEnumerable<Charge> GetCurrentQuarterCharges();
        IEnumerable<Charge> GetPreviousQuarterCharges();
        Task CreateCharge(Charge Charge);
    }
}
