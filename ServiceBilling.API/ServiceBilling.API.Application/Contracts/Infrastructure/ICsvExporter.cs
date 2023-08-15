using ServiceBilling.API.Application.Features.ServiceCategories.Queries.GetServiceCategoriesExport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBilling.API.Application.Contracts.Infrastructure
{
    public interface ICsvExporter
    {
        byte[] ExportServiceCategoriesToCsv(List<ServiceCategoriesExportDto> serviceCategoriesExportDtos);
    }
}
