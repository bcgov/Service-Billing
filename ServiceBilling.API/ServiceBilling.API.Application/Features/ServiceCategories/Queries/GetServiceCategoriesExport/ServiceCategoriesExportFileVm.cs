namespace ServiceBilling.API.Application.Features.ServiceCategories.Queries.GetServiceCategoriesExport
{
    public class ServiceCategoriesExportFileVm
    {
        public string ServiceCategoriesExportFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[]? Data { get; set; }
    }
}
