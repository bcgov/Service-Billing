using System.IO.Pipelines;

namespace ServiceBilling.BillingManagement.UI.Models
{
    public static class DbInitializer
    {
        public static void Seed(DataContext context)
        {
            if (!context.ServiceCategories.Any())
            {
                context.AddRange
                (
                    new ServiceCategory
                    {
                        Name = "Sample Category",
                        GdxBusinessArea = "CITZ",
                        Costs = "3.50",
                        Description = "Sample Category Description",
                        UOM = "Hr"
                    },
                    new ServiceCategory
                    {
                        Name = "Sample Category",
                        GdxBusinessArea = "CITZ",
                        Costs = "3.50",
                        Description = "Sample Category Description",
                        UOM = "Hr"
                    },
                    new ServiceCategory
                    {
                        Name = "Sample Category",
                        GdxBusinessArea = "CITZ",
                        Costs = "3.50",
                        Description = "Sample Category Description",
                        UOM = "Hr"
                    });
            }

            context.SaveChanges();


        }

    }

}
