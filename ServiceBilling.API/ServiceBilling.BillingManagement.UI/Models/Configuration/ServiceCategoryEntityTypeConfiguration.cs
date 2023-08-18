using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ServiceBilling.BillingManagement.UI.Models.Configuration
{
    public class ServiceCategoryEntityTypeConfiguration : IEntityTypeConfiguration<ServiceCategory>
    {
        public void Configure(EntityTypeBuilder<ServiceCategory> builder)
        {
            builder
                .Property(x => x.Name)
                .IsRequired();
        }
    }
}
