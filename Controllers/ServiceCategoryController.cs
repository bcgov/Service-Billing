
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Service_Billing.Filters;
using Service_Billing.Models;
using Service_Billing.Models.Repositories;
using Service_Billing.ViewModels;
using System.Globalization;

namespace Service_Billing.Controllers
{
    public class ServiceCategoryController : Controller
    {
        private readonly IServiceCategoryRepository _categoryRepository;
        private readonly IBillRepository _billRepository;
        private readonly ILogger<ServiceCategoryController> _logger;

        public ServiceCategoryController(ILogger<ServiceCategoryController> logger, 
            IServiceCategoryRepository categoryRepository,
            IBillRepository billRepository)
        {
            _categoryRepository = categoryRepository;
            _billRepository = billRepository;
            _logger = logger;
        }

        [ServiceFilter(typeof(GroupAuthorizeActionFilter))]
        public IActionResult Index(string areaFilter, string nameFilter, string activeFilter, string uomFilter, string ownerFilter)
        {
            ViewData["AreaFilter"] = areaFilter;
            ViewData["NameFilter"] = nameFilter;
            ViewData["IsActiveFilter"] = activeFilter;
            ViewData["UOMFilter"] = uomFilter;
            ViewData["OwnerFilter"] = ownerFilter;
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
            List<string> busAreas = new List<string>();
            if (categories != null && categories.Any())
            {
                foreach (ServiceCategory category in categories)
                {
                    if (!String.IsNullOrEmpty(category.GDXBusArea) && !busAreas.Contains(category.GDXBusArea))
                        busAreas.Add(category.GDXBusArea);
                }
            }

            ViewData["BusAreas"] = busAreas;
            categories = GetFilteredCategories(areaFilter, nameFilter, activeFilter, uomFilter, ownerFilter);

            return View(categories);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            ServiceCategory? serviceCategory = _categoryRepository.GetById(id);
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
                _categoryRepository.Update(serviceCategory);
                if(serviceCategory.UpdateCharges)
                {
                    await _billRepository.UpdateAllChargesForServiceCategory(serviceCategory.ServiceId);
                }
            }

            catch (DbUpdateException ex)
            {
                _logger.LogError(ex.Message);
            }
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
            return View("index", categories);
        }

        [ServiceFilter(typeof(GroupAuthorizeActionFilter))]
        [HttpGet]
        public IActionResult Create()
        {
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
            List<string> busAreas = new List<string>();
            if (categories != null && categories.Any())
            {
                foreach (ServiceCategory category in categories)
                {
                    if (!String.IsNullOrEmpty(category.GDXBusArea) && !busAreas.Contains(category.GDXBusArea))
                        busAreas.Add(category.GDXBusArea);
                }
            }
            CreateServiceViewModel model = new CreateServiceViewModel();
            model.BusArea = busAreas;

            return View(model);
        }

        [ServiceFilter(typeof(GroupAuthorizeActionFilter))]
        [HttpPost]
        public IActionResult Create(CreateServiceViewModel model)
        {
            try
            {
                int id = _categoryRepository.Add(model.Service);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"A database update exception occurred: {ex.Message}");

            }
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();

            return View("index", categories);
        }

        [HttpPost]
        public IActionResult Delete(ServiceCategory serviceCategory)
        {

            return View("index");
        }

        [HttpGet]
        public async Task<IActionResult> WriteToCSV(string areaFilter, string nameFilter, string activeFilter, string uomFilter, string ownerFilter)
        {
            IEnumerable<ServiceCategory> categories = GetFilteredCategories(areaFilter, nameFilter, activeFilter, uomFilter, ownerFilter);
            try
            {
                using var memoryStream = new MemoryStream();
                using (var streamWriter = new StreamWriter(memoryStream))
                {
                    using var csvWriter = new CsvWriter(streamWriter);
                    csvWriter.WriteRecords(categories);
                }
                string fileName = "Service-Categories";
                if (!String.IsNullOrEmpty(areaFilter))
                    fileName += $"-{areaFilter}";
                if (!String.IsNullOrEmpty(nameFilter))
                    fileName += $"-{nameFilter}";
                if (!String.IsNullOrEmpty(activeFilter))
                    fileName += $"-{activeFilter}";
                if (!String.IsNullOrEmpty(uomFilter))
                    fileName += $"-{uomFilter}";
                if (!String.IsNullOrEmpty(ownerFilter))
                    fileName += $"-{ownerFilter}";

                fileName += DateTime.Today.ToString("dd-mm-yyyy");
                fileName += ".csv";

                return File(memoryStream.ToArray(), "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                //we really need an error page
                return StatusCode(500);
            }
        }

        private IEnumerable<ServiceCategory> GetFilteredCategories(string areaFilter, string nameFilter, string activeFilter, string uomFilter, string ownerFilter)
        {
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
            if (!String.IsNullOrEmpty(areaFilter))
                categories = categories.Where(x => x.GDXBusArea == areaFilter);
            if (!String.IsNullOrEmpty(nameFilter))
                categories = categories.Where(x => x.Name.ToLower().Contains(nameFilter.ToLower()));
            if (activeFilter != null)
            {
                if (activeFilter == "active")
                    categories = categories.Where(x => x.IsActive == true);
                else if (activeFilter == "inactive")
                    categories = categories.Where(x => x.IsActive == false);
            }
            if (!String.IsNullOrEmpty(uomFilter))
                categories = categories.Where(x => x.UOM == uomFilter);
            if (!String.IsNullOrEmpty(ownerFilter))
                categories = categories.Where(x => x.ServiceOwner != null && x.ServiceOwner.ToLower().Contains(ownerFilter.ToLower()));
            return categories;
        }
    }
}
