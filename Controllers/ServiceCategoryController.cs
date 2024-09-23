
using ClosedXML.Excel;
using CsvHelper;
using DocumentFormat.OpenXml.Vml;
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
        private readonly IBusinessAreaRepository _businessAreaRepository;
        private readonly ILogger<ServiceCategoryController> _logger;

        public ServiceCategoryController(ILogger<ServiceCategoryController> logger, 
            IServiceCategoryRepository categoryRepository,
            IBillRepository billRepository,
            IBusinessAreaRepository businessAreaRepository)
        {
            _categoryRepository = categoryRepository;
            _billRepository = billRepository;
            _logger = logger;
            _businessAreaRepository = businessAreaRepository;
        }

        [ServiceFilter(typeof(GroupAuthorizeActionFilter))]
        public IActionResult Index(int areaFilter, string nameFilter, string uomFilter, string ownerFilter, string activeFilter = "active")
        {
            ViewData["AreaFilter"] = areaFilter;
            ViewData["NameFilter"] = nameFilter;
            ViewData["IsActiveFilter"] = activeFilter;
            ViewData["UOMFilter"] = uomFilter;
            ViewData["OwnerFilter"] = ownerFilter;
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
            IEnumerable<BusinessArea> busAreas = _businessAreaRepository.GetAll();
          

            ViewData["BusAreas"] = busAreas;
            categories = GetFilteredCategories(areaFilter, nameFilter, activeFilter, uomFilter, ownerFilter);

            return View(categories);
        }

        [HttpGet]
        public IActionResult Details(int id, bool isNew = false, bool isEdited = false)
        {
            ServiceCategory serviceCategory = _categoryRepository.GetById(id);
            ViewData["isNew"] = isNew;
            ViewData["isEdited"] = isEdited;
            return View(serviceCategory);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            ServiceCategory? serviceCategory = _categoryRepository.GetById(id);
            if (serviceCategory == null)
            {
                return NotFound();
            }
            serviceCategory.BusArea = _businessAreaRepository.GetById(serviceCategory.BusAreaId); //I don't understand why this isn't handled as a foreign relationship
            return View(serviceCategory);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ServiceCategory serviceCategory)
        {
            try
            {
                serviceCategory.BusArea = _businessAreaRepository.GetById(serviceCategory.BusAreaId);
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
            IEnumerable<BusinessArea> busAreas = _businessAreaRepository.GetAll();
            ViewData["BusAreas"] = busAreas;

            return RedirectToAction("details", new { id = serviceCategory.ServiceId, isEdited = true });
        }

        [ServiceFilter(typeof(GroupAuthorizeActionFilter))]
        [HttpGet]
        public IActionResult Create()
        {
            List<BusinessArea> busAreas = _businessAreaRepository.GetAll().ToList();
            CreateServiceViewModel model = new CreateServiceViewModel();
            model.BusAreas = busAreas;

            return View(model);
        }

        [ServiceFilter(typeof(GroupAuthorizeActionFilter))]
        [HttpPost]
        public IActionResult Create(CreateServiceViewModel model)
        {
            try
            {
                if(model.Service.ServiceId <= 0 && !String.IsNullOrEmpty(model.NewBusAreaAcronym) 
                    && !String.IsNullOrEmpty(model.NewBusAreaName)) //Oh boy; a new business area!
                {
                    BusinessArea businessArea = new BusinessArea(model.NewBusAreaAcronym, model.NewBusAreaName);
                    model.Service.BusAreaId = _businessAreaRepository.Add(businessArea);
                }

                int id = _categoryRepository.Add(model.Service);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"A database update exception occurred: {ex.Message}");

            }
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
            IEnumerable<BusinessArea> busAreas = _businessAreaRepository.GetAll();
            ViewData["BusAreas"] = busAreas;
            return RedirectToAction("details", new { id = model.Service.ServiceId, isNew = true });
        }

        [HttpPost]
        public IActionResult Delete(ServiceCategory serviceCategory)
        {

            return View("index");
        }

        [HttpGet]
        public IActionResult WriteToExcel(int areaFilter, string nameFilter, string activeFilter, string uomFilter, string ownerFilter)
        {
            IEnumerable<ServiceCategory> categories = GetFilteredCategories(areaFilter, nameFilter, activeFilter, uomFilter, ownerFilter);
            try
            {
                string fileName = "Service-Categories";
                //if (!String.IsNullOrEmpty(areaFilter)) TODO: get business area acronym based on the id, and add it to the filename
                //    fileName += $"-{areaFilter}";
                if(areaFilter > 0)
                {
                    BusinessArea? area = _businessAreaRepository.GetById(areaFilter);
                    if (area == null)
                        throw new Exception($"Somehow an Excel file made to be written based off a business area that doesn't exist. Business area ID: {areaFilter}");
                    fileName += $"-{area.Acronym}";
                }
                if (!String.IsNullOrEmpty(nameFilter))
                    fileName += $"-{nameFilter}";
                if (!String.IsNullOrEmpty(activeFilter))
                    fileName += $"-{activeFilter}";
                if (!String.IsNullOrEmpty(uomFilter))
                    fileName += $"-{uomFilter}";
                if (!String.IsNullOrEmpty(ownerFilter))
                    fileName += $"-{ownerFilter}";

                fileName += DateTime.Today.ToString("dd-mm-yyyy");
                fileName += ".xlsx";

                using var wb = new XLWorkbook();
                var ws = wb.AddWorksheet();
                List<ChargeRow> rows = new List<ChargeRow>();
                ws.Cell("A1").InsertTable(categories);
                ws.Column(9).Delete(); // don't need "UpdateCharges?"
                // Adjust column size to contents.
                ws.Columns().AdjustToContents();
                using var stream = new MemoryStream();
                wb.SaveAs(stream);
                var content = stream.ToArray();
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return File(content, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                //we really need an error page
                return StatusCode(500);
            }
        }

        private IEnumerable<ServiceCategory> GetFilteredCategories(int areaFilter, string nameFilter, string activeFilter, string uomFilter, string ownerFilter)
        {
            IEnumerable<ServiceCategory> categories = _categoryRepository.GetAll();
            if (areaFilter > 0)
                categories = categories.Where(x => x.BusAreaId == areaFilter);
            if (!String.IsNullOrEmpty(nameFilter))
                categories = categories.Where(x => !String.IsNullOrEmpty(x.Name) && x.Name.ToLower().Contains(nameFilter.ToLower()));
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
