
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Service_Billing.Models;
using Service_Billing.Models.Repositories;
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

        //public IActionResult Details(int id)
        //{
        //    ServiceCategory? serviceCategory = _categoryRepository.GetById(id);
        //    if (serviceCategory == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(serviceCategory);
        //}

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
    }
}
