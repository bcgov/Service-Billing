using CsvHelper;
using Microsoft.IdentityModel.Protocols.Configuration;
using NuGet.Common;
using Service_Billing.Models;

namespace Service_Billing.ViewModels
{
    public class GeneratedReportViewModel
    {
        public SortedDictionary<string, decimal> ServicesAndSums { get; set; }
        public string BillingQuarter { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Ministry { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public int ServiceCategoryId { get; set; }
        public string Authority { get; set; } = string.Empty;
        public int ClientNumber { get; set; }
    }
}
