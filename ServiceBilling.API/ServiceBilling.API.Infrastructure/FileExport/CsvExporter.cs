using CsvHelper;
using ServiceBilling.API.Application.Contracts.Infrastructure;
using ServiceBilling.API.Application.Features.ServiceCategories.Queries.GetServiceCategoriesExport;
using System.Formats.Asn1;

namespace ServiceBilling.API.Infrastructure.FileExport
{
    public class CsvExporter : ICsvExporter
    {
        public byte[] ExportServiceCategoriesToCsv(List<ServiceCategoriesExportDto> serviceCategoriesExportDtos)
        {
            using var memoryStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memoryStream))
            {
                using var csvWriter = new CsvWriter(streamWriter);
                csvWriter.WriteRecords(serviceCategoriesExportDtos);
            }

            return memoryStream.ToArray();
        }
    }
}
