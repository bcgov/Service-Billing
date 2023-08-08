
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Service_Billing.Models;
using Service_Billing.ViewModels;

namespace Service_Billing.Controllers
{
    public class ServiceCategoryController: Controller
    {
        private readonly IServiceCategoryRepository _categoryRepository;

        public ServiceCategoryController(IServiceCategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public IActionResult Index()
        {
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
            return View(categories);
        }
    }
}
