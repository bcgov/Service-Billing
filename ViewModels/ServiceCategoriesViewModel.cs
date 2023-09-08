using CsvHelper;
using Microsoft.AspNetCore.Mvc;
using Service_Billing.Models;

namespace Service_Billing.ViewModels
{
    public class ServiceCategoriesViewModel
    {
        public IEnumerable<ServiceCategory> Categories { get; set; }
        //public Task<IActionResult> WriteToCSV(ServiceCategoriesViewModel model)
        //{
        //    try
        //    {
        //        using var memoryStream = new MemoryStream();
        //        using (var streamWriter = new StreamWriter(memoryStream))
        //        {
        //            using var csvWriter = new CsvWriter(streamWriter);
        //            csvWriter.WriteRecords(model.Categories);
        //        }

        //        return File(memoryStream.ToArray(), "application/octet-stream");
        //    }
        //    catch (Exception ex)
        //    {
        //        //we really need an error page
        //        return null;
        //    }
        //}
    }
}
