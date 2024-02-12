using Microsoft.EntityFrameworkCore;
using Service_Billing.Models;
using System.Text.Json;
using File = System.IO.File;

namespace Service_Billing.Data
{
    public static class DbInitializer
    {
        public static void SeedMinistries(ServiceBillingContext context)
        {
            if (context.Ministries.Any())
            {
                context.Ministries.RemoveRange(context.Ministries);
                context.SaveChanges();
            }

            var ministryData = File.ReadAllText("./Data/MinistrySeedData.json");
            var ministries = JsonSerializer.Deserialize<List<Ministry>>(ministryData);

            using (var transaction = context.Database.BeginTransaction())
            {
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Ministries ON");

                foreach (var ministry in ministries!)
                {
                    context.Ministries.Add(ministry);
                }

                context.SaveChanges();
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Ministries OFF");

                transaction.Commit();
            }
        }

        public static void SeedServices(ServiceBillingContext context)
        {
            if (context.ServiceCategories.Any())
            {
                context.ServiceCategories.RemoveRange(context.ServiceCategories);
                context.SaveChanges();
            }

            var serviceData = File.ReadAllText("./Data/ServiceCategorySeedData.json");
            var services = JsonSerializer.Deserialize<List<ServiceCategory>>(serviceData);

            using (var transaction = context.Database.BeginTransaction())
            {
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.ServiceCategories ON");

                foreach (var service in services!)
                {
                    context.ServiceCategories.Add(service);
                }

                context.SaveChanges();
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.ServiceCategories OFF");

                transaction.Commit();
            }
        }

     
        public static void SeedAccounts(ServiceBillingContext context)
        {
            if (context.ClientAccounts.Any())
            {
                context.ClientAccounts.RemoveRange(context.ClientAccounts);
                context.SaveChanges();
            }

            var accountData = File.ReadAllText("./Data/ClientAccountSeedData.json");
            var accounts = JsonSerializer.Deserialize<List<ClientAccount>>(accountData);

            using (var transaction = context.Database.BeginTransaction())
            {
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.ClientAccounts ON");

                foreach (var account in accounts!)
                {
                    context.ClientAccounts.Add(account);
                }

                context.SaveChanges();
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.ClientAccounts OFF");

                transaction.Commit();
            }
        }

        public static void SeedCharges(ServiceBillingContext context)
        {
            if (context.Bills.Any())
            {
                context.Bills.RemoveRange(context.Bills);
                context.SaveChanges();
            }

            var chargeData = File.ReadAllText("./Data/ChargeSeedData.json");
            var charges = JsonSerializer.Deserialize<List<Bill>>(chargeData);

            using (var transaction = context.Database.BeginTransaction())
            {
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Bills ON");

                foreach (var charge in charges!)
                {
                    context.Bills.Add(charge);
                }

                context.SaveChanges();
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT dbo.Bills OFF");

                transaction.Commit();
            }
        }
    }
}
