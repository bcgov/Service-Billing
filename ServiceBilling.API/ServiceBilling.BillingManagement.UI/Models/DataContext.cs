using Microsoft.EntityFrameworkCore;
using ServiceBilling.BillingManagement.UI.Models.Repositories;

namespace ServiceBilling.BillingManagement.UI.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<Charge> Charges { get; set; }
        public DbSet<ClientAccount> ClientAccounts { get; set; }
        public DbSet<ClientTeam> ClientTeams { get; set; }
        public DbSet<Ministry> Ministries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof (DataContext).Assembly);

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServiceCategory>()
                .Property(s => s.Name)
                .IsRequired();
        }
    }
}
