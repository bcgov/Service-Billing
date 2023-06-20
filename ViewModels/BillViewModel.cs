using Microsoft.AspNetCore.Mvc.Rendering;
using Service_Billing.Models;

namespace Service_Billing.ViewModels
{
    public class BillViewModel
    {
        public Bill bill { get; }
        public IEnumerable<ServiceCategory> ServiceCategories { get; }
        public IEnumerable<ClientAccount> ClientAccounts { get; }
        public List<SelectListItem> serviceCategories { set; get; }
        public List<ServiceCategory> categoriesList { set; get; }

        public SelectListItem selectedCategory { get; set; }

        public BillViewModel(Bill bill, IEnumerable<ServiceCategory> serviceCategories)
        {
            this.bill = bill;
            ServiceCategories = serviceCategories;
            categoriesList = serviceCategories.ToList();
        }
    }
}
