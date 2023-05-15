using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Service_Billing.Models;

namespace Service_Billing.Data
{
    public class Service_BillingContext : DbContext
    {
        public Service_BillingContext (DbContextOptions<Service_BillingContext> options)
            : base(options)
        {
        }

        public DbSet<Service_Billing.Models.BillEntries> billingData { get; set; } = default!;
    }
}
