using Service_Billing.Models;

namespace Service_Billing.ViewModels
{
    public class CreateServiceViewModel
    {
        public ServiceCategory Service { get; set; }
        public List<string> BusArea { get; set; }

        public CreateServiceViewModel()
        { 
            BusArea = new List<string>();
            Service = new ServiceCategory();
        }
    }
}
