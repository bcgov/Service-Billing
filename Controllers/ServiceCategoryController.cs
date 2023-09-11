
using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Service_Billing.Models;
using Service_Billing.Models.Repositories;
using Service_Billing.ViewModels;
using System.Globalization;

namespace Service_Billing.Controllers
{
    public class ServiceCategoryController: Controller
    {
        private readonly IServiceCategoryRepository _categoryRepository;

        public ServiceCategoryController(IServiceCategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public IActionResult Index(string areaFilter, string nameFilter, string activeFilter, string uomFilter, string ownerFilter)
        {
            ViewData["AreaFilter"] = areaFilter;
            ViewData["NameFilter"] = nameFilter;
            ViewData["IsActiveFilter"] = activeFilter;
            ViewData["UOMFilter"] = uomFilter;
            ViewData["OwnerFilter"] = ownerFilter;
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
            List<string> busAreas = new List<string>();
            
            foreach(ServiceCategory category in categories)
            {
                if(!String.IsNullOrEmpty(category.GDXBusArea) && !busAreas.Contains(category.GDXBusArea))
                    busAreas.Add(category.GDXBusArea);
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
        public IActionResult Edit(ServiceCategory serviceCategory)
        {
            try
            {
                _categoryRepository.Update(serviceCategory);  
            }
            catch (DbUpdateException ex)
            {

            }
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
            return View("index", categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
          
            return View();
        }

        [HttpPost]
        public IActionResult Create(ServiceCategory serviceCategory)
        {
            try
            {
               int id = _categoryRepository.Add(serviceCategory);
            }
            catch (DbUpdateException ex)
            {

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
                categories = categories.Where(x => x.Name == nameFilter);
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
