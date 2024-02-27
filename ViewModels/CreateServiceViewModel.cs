using Service_Billing.Models;

namespace Service_Billing.ViewModels
{
    public class CreateServiceViewModel
    {
        public ServiceCategory Service { get; set; }
        public IEnumerable<BusinessArea> BusAreas { get; set; }

        public CreateServiceViewModel()
        { 
            Service = new ServiceCategory();
        }
    }
}
