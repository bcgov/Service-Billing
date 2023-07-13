namespace ServiceBilling.API.Application.Features.ServiceCategories.Queries.GetServiceCategoryList
{
    public class ServiceCategoryListVm
    {
        public Guid ServiceCategoryId { get; set; }
        public string Name { get; set; }
        public string GdxBusinessArea { get; set; }
        public string Costs { get; set; }
        public string Description { get; set; }
        public string UOM { get; set; }
    }
}
