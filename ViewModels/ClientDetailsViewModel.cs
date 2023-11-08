using Service_Billing.Models;

namespace Service_Billing.ViewModels
{
    public class ClientDetailsViewModel
    {
        public ClientAccount Account { get; set; }
        public ClientTeam? Team { get; set; }
        public IEnumerable<Bill> Charges { get; set; }
        public IEnumerable<ServiceCategory> Categories { get; set; }

        public Dictionary<int, string> CategoryNames { get; set; }
        public ClientDetailsViewModel(ClientAccount account, ClientTeam team, IEnumerable<Bill> charges, IEnumerable<ServiceCategory> categories) 
        {
            Account = account;
            Team = team;
            Charges = charges;
            Categories = categories;
            CategoryNames = new Dictionary<int, string>();
            BuildCategoriesDict();
        }

        private void BuildCategoriesDict()
        {
            if(Charges.Any() && Categories.Any()) 
            {
                foreach(Bill bill in Charges)
                {
                    string categoryName = Categories.FirstOrDefault(x => x.ServiceId == bill.ServiceCategoryId).Name;
                    CategoryNames.Add(bill.Id, categoryName);
                }
            }
        }
    }
}
