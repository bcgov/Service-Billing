using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        //public DbSet<Service_Billing.Models.BillEntries> billingData { get; set; } = default!;
        public DbSet<Bill> bills { get; set; } = default!;
        public DbSet<ClientAccount> clientAccounts { get; set; }
        public DbSet<ClientTeam> clientTeams { get; set; }
        public DbSet<ServiceCategory> serviceCategories { get; set; }
    }
}
