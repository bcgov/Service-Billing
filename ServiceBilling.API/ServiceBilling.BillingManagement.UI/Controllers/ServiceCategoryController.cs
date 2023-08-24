using CsvHelper;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<FileResult> Export()
        {
            var serviceCategories = (await _serviceCategoryRepository.GetAllServiceCategoriesAsync()).ToList();

            var fileData = ExportServiceCategoriesToCsv(serviceCategories);

            return File(fileData, "text/csv", $"{Guid.NewGuid()}.csv");
        }

        public byte[] ExportServiceCategoriesToCsv(List<ServiceCategory> serviceCategories)
        {
            using var memoryStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                using var csvWriter = new CsvWriter(streamWriter);
                csvWriter.WriteRecords(serviceCategories);
            }

            return memoryStream.ToArray();
        }
    }
}
