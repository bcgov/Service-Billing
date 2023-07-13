using Microsoft.EntityFrameworkCore;
using ServiceBilling.API.Domain.Entities;

namespace ServiceBilling.API.Persistence
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<ClientAccount> ClientAccounts { get; set; }
        public DbSet<ClientTeam> ClientTeams { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof 
                (DataContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
