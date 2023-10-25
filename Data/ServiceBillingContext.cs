using Microsoft.EntityFrameworkCore;
using Service_Billing.Models;

namespace Service_Billing.Data
{
    public class ServiceBillingContext : DbContext
    {
        public ServiceBillingContext (DbContextOptions<ServiceBillingContext> options)
            : base(options)
        {
        }

        public DbSet<Bill> Bills { get; set; } = default!;
        public DbSet<ClientAccount> ClientAccounts { get; set; }
        public DbSet<ClientTeam> ClientTeams { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<Ministry> Ministries { get; set; }
    }
}
