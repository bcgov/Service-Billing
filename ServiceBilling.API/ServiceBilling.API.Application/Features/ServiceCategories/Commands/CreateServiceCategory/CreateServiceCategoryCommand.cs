using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceBilling.API.Application.Features.ServiceCategories.Commands.CreateServiceCategory
{
    public class CreateServiceCategoryCommand : IRequest<Guid>
    {
        public string Name { get; set; }
        public string GdxBusinessArea { get; set; }
        public string Costs { get; set; }
        public string Description { get; set; }
        public string UOM { get; set; }
    }
}
