using Service_Billing.Models;

namespace Service_Billing.ViewModels
{
    public class ClientIntakeViewModel
    {
        public int Id { get; set; }
        public ClientTeam Team { get; set; }
        public ClientAccount Account { get; set; }
        public string MinistryAcronym { get; set; }
        public string DivisionOrBranch { get; set; }

        public ClientIntakeViewModel()
        {
            Team = new ClientTeam();
            Account = new ClientAccount();
        }
    }
}
