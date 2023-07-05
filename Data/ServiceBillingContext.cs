using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Service_Billing.Models;
using Service_Billing.ViewModels;

namespace Service_Billing.Data
{
    public class ServiceBillingContext : DbContext
    {
        public ServiceBillingContext (DbContextOptions<ServiceBillingContext> options)
            : base(options)
        {
        }

        //public DbSet<Service_Billing.Models.BillEntries> billingData { get; set; } = default!;
        public DbSet<Bill> Bills { get; set; } = default!;
        public DbSet<ClientAccount> ClientAccounts { get; set; }
        public DbSet<ClientTeam> ClientTeams { get; set; }
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<Service_Billing.ViewModels.ClientIntakeViewModel> ClientIntakeViewModel { get; set; } = default!;
    }
}
