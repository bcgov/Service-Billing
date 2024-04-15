using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using Service_Billing.Models;
using System.Linq;
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

        public static void SeedPeople(ServiceBillingContext context)
        {
            if (context.People.Any())
            {
                context.People.RemoveRange(context.People);
                context.SaveChanges();
                context.ChangeTracker.Clear(); // Clear the change tracker
            }

            var peopleData = File.ReadAllText("./Data/PeopleSeedData.json");
            var people = JsonSerializer.Deserialize<List<Person>>(peopleData);

            var processedIds = new HashSet<string>(); // To keep track of processed IDs

            using (var transaction = context.Database.BeginTransaction())
            {
                foreach (var person in people!)
                {
                    if (person == null || processedIds.Contains(person.Id.ToString()))
                        continue;

                    var existingEntity = context.People.AsNoTracking().FirstOrDefault(p => p.Id == person.Id);

                    if (existingEntity == null)
                    {
                        context.People.Add(person);
                        processedIds.Add(person.Id.ToString()); // Mark this ID as processed
                    }
                    else
                    {
                        // Handle updates or simply skip if exact duplicates are not needed
                    }
                }

                context.SaveChanges();
                transaction.Commit();
            }
        }

    }
}
