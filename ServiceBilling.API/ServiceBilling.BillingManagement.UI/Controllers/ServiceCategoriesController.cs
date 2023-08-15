using Microsoft.AspNetCore.Mvc;
using ServiceBilling.API.Application.Features.ServiceCategories.Queries.GetServiceCategoryList;
using System.Text.Json;

namespace ServiceBilling.BillingManagement.UI.Controllers
{
    public class ServiceCategoriesController : Controller
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public async Task<IActionResult> Index()
        {
            HttpResponseMessage response = await _httpClient.GetAsync("http://localhost:5028/api/ServiceCategory/all");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var serviceCategories = JsonSerializer.Deserialize<List<ServiceCategoryListVm>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true // Make property name matching case-insensitive
                });
                return View(serviceCategories);
            }

            return View();
        }
    }
}
