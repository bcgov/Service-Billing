using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceBilling.API.Domain.Entities;

namespace ServiceBilling.API.Persistence.Configuration
{
    public class ClientAccountConfiguration : IEntityTypeConfiguration<ClientAccount>
    {
        public void Configure(EntityTypeBuilder<ClientAccount> builder)
        {
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(50);

        }
    }
}
