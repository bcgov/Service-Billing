using Microsoft.EntityFrameworkCore;
using Moq;
using Service_Billing.Data;
using Service_Billing.Models;
using Service_Billing.Models.Repositories;
using Xunit;

namespace ServiceBillingUnitTests.Repositories
{
    public class BillRepositoryTests
    {
        [Fact]
        public async Task PromoteChargesToNewQuarter_ShouldPromoteEligibleBills_IntegrationTest()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ServiceBillingContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Create the in-memory context
            using (var context = new ServiceBillingContext(options))
            {
                // Seed data into the in-memory database
                context.FiscalPeriods.Add(new FiscalPeriod { Id = 259, Period = "Fiscal 23/24 Quarter 3" });
                context.FiscalPeriods.Add(new FiscalPeriod { Id = 260, Period = "Fiscal 23/24 Quarter 4" });

                context.ServiceCategories.Add(new ServiceCategory
                {
                    ServiceId = 101,
                    Name = "Service A",
                    Costs = "400.0",
                    UOM = "Month",
                    IsActive = true
                });

                await context.SaveChangesAsync();

                context.Bills.Add(new Bill
                {
                    Id = 1,
                    ServiceCategoryId = 101,
                    Amount = 400,
                    Quantity = 3,
                    IsActive = true,
                    EndDate = null,
                    CurrentFiscalPeriodId = 259,
                    ServiceCategory = context.ServiceCategories.First(),
                    MostRecentActiveFiscalPeriod = context.FiscalPeriods.First(fp => fp.Id == 259)
                });

                await context.SaveChangesAsync();

                var mockLogger = new Mock<ILogger<BillRepository>>();
                var fiscalPeriodRepository = new FiscalPeriodRepository(context);
                var fiscalHistoryRepository = new FiscalHistoryRepository(context, mockLogger.Object);
                var changeLogRepository = new ChangeLogRepository(context, mockLogger.Object);

                var billRepository = new BillRepository(context, fiscalPeriodRepository, mockLogger.Object, fiscalHistoryRepository, changeLogRepository);

                // Act
                await billRepository.PromoteChargesToNewQuarter();

                // Assert
                var updatedBill = await context.Bills.FirstOrDefaultAsync(b => b.Id == 1);
                Assert.Equal(261, updatedBill.CurrentFiscalPeriodId);
            }
        }

        [Fact]
        public async Task PromoteChargesToNewQuarter_ShouldNotPromoteBills_WithEndDateBeforeQuarterStart()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ServiceBillingContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var context = new ServiceBillingContext(options))
            {
                context.FiscalPeriods.Add(new FiscalPeriod { Id = 259, Period = "Fiscal 23/24 Quarter 3" });
                context.FiscalPeriods.Add(new FiscalPeriod { Id = 260, Period = "Fiscal 23/24 Quarter 4" });

                context.ServiceCategories.Add(new ServiceCategory
                {
                    ServiceId = 101,
                    Name = "Service A",
                    Costs = "400.0",
                    UOM = "Month",
                    IsActive = true
                });

                await context.SaveChangesAsync();

                context.Bills.Add(new Bill
                {
                    Id = 1,
                    ServiceCategoryId = 101,
                    Amount = 400,
                    Quantity = 3,
                    IsActive = true,
                    EndDate = new DateTime(2023, 12, 31), // Ended before the new quarter
                    CurrentFiscalPeriodId = 259,
                    ServiceCategory = context.ServiceCategories.First(),
                    MostRecentActiveFiscalPeriod = context.FiscalPeriods.First(fp => fp.Id == 259)
                });

                await context.SaveChangesAsync();

                var mockLogger = new Mock<ILogger<BillRepository>>();
                var fiscalPeriodRepository = new FiscalPeriodRepository(context);
                var fiscalHistoryRepository = new FiscalHistoryRepository(context, mockLogger.Object);
                var changeLogRepository = new ChangeLogRepository(context, mockLogger.Object);

                var billRepository = new BillRepository(context, fiscalPeriodRepository, mockLogger.Object, fiscalHistoryRepository, changeLogRepository);
                // Act
                await billRepository.PromoteChargesToNewQuarter();

                // Assert
                var updatedBill = await context.Bills.FirstOrDefaultAsync(b => b.Id == 1);
                Assert.Equal(259, updatedBill.CurrentFiscalPeriodId); // Should remain in the old period
            }
        }

        [Fact]
        public async Task PromoteChargesToNewQuarter_ShouldNotPromoteInactiveBills_WithNullEndDate()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ServiceBillingContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using (var context = new ServiceBillingContext(options))
            {
                context.FiscalPeriods.Add(new FiscalPeriod { Id = 259, Period = "Fiscal 23/24 Quarter 3" });
                context.FiscalPeriods.Add(new FiscalPeriod { Id = 260, Period = "Fiscal 23/24 Quarter 4" });

                context.ServiceCategories.Add(new ServiceCategory
                {
                    ServiceId = 101,
                    Name = "Service A",
                    Costs = "400.0",
                    UOM = "Month",
                    IsActive = true
                });

                await context.SaveChangesAsync();

                context.Bills.Add(new Bill
                {
                    Id = 1,
                    ServiceCategoryId = 101,
                    Amount = 400,
                    Quantity = 3,
                    IsActive = false, // Bill is inactive
                    EndDate = null,
                    CurrentFiscalPeriodId = 259,
                    ServiceCategory = context.ServiceCategories.First(),
                    MostRecentActiveFiscalPeriod = context.FiscalPeriods.First(fp => fp.Id == 259)
                });

                await context.SaveChangesAsync();

                var mockLogger = new Mock<ILogger<BillRepository>>();
                var fiscalPeriodRepository = new FiscalPeriodRepository(context);
                var fiscalHistoryRepository = new FiscalHistoryRepository(context, mockLogger.Object);
                var changeLogRepository = new ChangeLogRepository(context, mockLogger.Object);

                var billRepository = new BillRepository(context, fiscalPeriodRepository, mockLogger.Object, fiscalHistoryRepository, changeLogRepository);

                // Act
                await billRepository.PromoteChargesToNewQuarter();

                // Assert
                var updatedBill = await context.Bills.FirstOrDefaultAsync(b => b.Id == 1);
                Assert.Equal(259, updatedBill.CurrentFiscalPeriodId); // Should remain in the old period
            }
        }
    }
}
