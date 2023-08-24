using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ServiceBilling.BillingManagement.UI.Models;
using ServiceBilling.BillingManagement.UI.Models.Repositories;
using ServiceBilling.BillingManagement.UI.ViewModels;

namespace ServiceBilling.BillingManagement.UI.Controllers
{
    public class ServiceCategoryController : Controller
    {
        private readonly IServiceCategoryRepository _serviceCategoryRepository;

        public ServiceCategoryController(IServiceCategoryRepository serviceCategoryRepository)
        {
            _serviceCategoryRepository = serviceCategoryRepository;
        }

        public async Task<IActionResult> Index()
        {
            ServiceCategoryListViewModel model = new ServiceCategoryListViewModel
            {
                ServiceCategories = (await _serviceCategoryRepository.GetAllServiceCategoriesAsync()).ToList()
            };

            return View(model);
        }

        //public async Task<IActionResult> Details()
        //{
        //}

        public async Task<IActionResult> Add()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add([Bind("Name,GdxBusinessArea,Costs,Description,UOM")] ServiceCategory serviceCategory)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    await _serviceCategoryRepository.AddServiceCategoryAsync(serviceCategory);
                    return RedirectToAction(nameof(Index));
                }

            }
            catch (Exception ex) 
            {
                ModelState.AddModelError("", $"Adding the service category, please try again! Error: {ex.Message}");    
            }

            return View();
        }

        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            ServiceCategory? serviceCategory = _serviceCategoryRepository.GetById(id);
            if (serviceCategory == null)
            {
                return NotFound();
            }

            return View(serviceCategory);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ServiceCategory serviceCategory)
        {
            try
            {
                _serviceCategoryRepository.Update(serviceCategory);
            }
            catch (DbUpdateException ex)
            {

            }
            ServiceCategoryListViewModel model = new ServiceCategoryListViewModel
            {
                ServiceCategories = (await _serviceCategoryRepository.GetAllServiceCategoriesAsync()).ToList()
            };
            return View("index", model);
        }

    }
}
