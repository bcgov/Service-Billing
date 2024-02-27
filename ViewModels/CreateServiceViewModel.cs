using Service_Billing.Models;

namespace Service_Billing.ViewModels
{
    public class CreateServiceViewModel
    {
        public ServiceCategory Service { get; set; }
        public List<BusinessArea> BusAreas { get; set; } = new List<BusinessArea>();

        public string? NewBusAreaAcronym { get; set; }
        public string? NewBusAreaName { get; set; }

        public CreateServiceViewModel()
        { 
            Service = new ServiceCategory();
        }
    }
}
