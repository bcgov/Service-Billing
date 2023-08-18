using Microsoft.EntityFrameworkCore;

namespace ServiceBilling.BillingManagement.UI.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<ServiceCategory> ServiceCategories { get; set; }

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
