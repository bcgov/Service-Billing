using Moq;
using Service_Billing.Models;
using Service_Billing.Models.Repositories;

namespace ServiceBillingUnitTests.Mocks
{
    public class RepositoryMocks
    {
        public static Mock<IPeopleRepository> GetPeopleRepository()
        {
            var people = new List<Person>
            {
                new Person {
                    Id = 28,
                    DisplayName = "Lashley, Andre CITZ:EX",
                    Mail = "Andre.Lashley@gov.bc.ca",
                    Name = "André Lashley" },
            };

            var mockRepository = new Mock<IPeopleRepository>();
            mockRepository.Setup(repo => repo.AllPeople).Returns(people);

            return mockRepository;
        }

        public static Mock<IFiscalPeriodRepository> GetFiscalPeriodRepository()
        {
            var fiscalPeriods = new List<FiscalPeriod>
            {
                new FiscalPeriod
                {
                    Id = 259,
                    Period = "Fiscal 23/24 Quarter 3"
                },
                new FiscalPeriod
                {
                    Id = 260,
                    Period = "Fiscal 23/24 Quarter 4"
                },
                new FiscalPeriod
                {
                    Id = 261,
                    Period = "Fiscal 24/25 Quarter 1"
                },
                new FiscalPeriod
                {
                    Id = 262,
                    Period = "Fiscal 24/25 Quarter 2"
                },
                new FiscalPeriod
                {
                    Id = 263,
                    Period = "Fiscal 24/25 Quarter 3"
                },
            };

            var mockRepository = new Mock<IFiscalPeriodRepository>();
            mockRepository.Setup(repo => repo.GetFiscalPeriodByString(It.IsAny<string>()))
                .Returns((string period) => fiscalPeriods.FirstOrDefault(fp => fp.Period == period));
            mockRepository.Setup(repo => repo.GetByFiscalQuarterString(It.IsAny<string>()))
                .Returns((string period) => fiscalPeriods.FirstOrDefault(fp => fp.Period == period));
            mockRepository.Setup(repo => repo.GetFiscalPeriodById(It.IsAny<int>()))
           .Returns((int id) => fiscalPeriods.FirstOrDefault(fp => fp.Id == id));

            return mockRepository;
        }

        public static Mock<IBusinessAreaRepository> GetBusinessAreaRepository()
        {
            var businessAreas = new List<BusinessArea>
            {
                new BusinessArea
                {
                    Id = 1,
                    Name = "Analytics",
                    Acronym = "ANA",
                },
                new BusinessArea
                {
                    Id = 2,
                    Name = "Any",
                    Acronym = "Any",
                },
                new BusinessArea
                {
                    Id = 3,
                    Name = "Digital Engagement Solutions",
                    Acronym = "DES",
                },
                new BusinessArea
                {
                    Id = 4,
                    Name = "Delivery Management Services",
                    Acronym = "DMS",
                },
                new BusinessArea
                {
                    Id = 5,
                    Name = "Multi",
                    Acronym = "Multi",
                },
                new BusinessArea
                {
                    Id = 6,
                    Name = "Other",
                    Acronym = "Other",
                },
                new BusinessArea
                {
                    Id = 7,
                    Name = "Online Service Solutions",
                    Acronym = "OSS",
                }
            };

            var mockRepository = new Mock<IBusinessAreaRepository>();
            mockRepository.Setup(repo => repo.GetAll())
                .Returns(businessAreas.OrderBy(x => x.Acronym));
            mockRepository.Setup(repo => repo.GetById(It.IsAny<int>()))
                .Returns((int id) => businessAreas.FirstOrDefault(ba => ba.Id == id));

            return mockRepository;
        }

        public static Mock<IServiceCategoryRepository> GetServiceCategoryRepository()
        {
            var serviceCategories = new List<ServiceCategory>
            {
               new ServiceCategory
                {
                    ServiceId = 1,
                    Name = "CMS Lite licence",
                    Costs = "0.0",
                    Description = "CMS Lite licence",
                    IsActive = false,
                    UOM = "Each",
                    ServiceOwner = null,
                    BusAreaId = 7
                },
                new ServiceCategory
                {
                    ServiceId = 2,
                    Name = "CMS Lite introductory training",
                    Costs = null,
                    Description = "1 day training session",
                    IsActive = false,
                    UOM = "Each",
                    ServiceOwner = null,
                    BusAreaId = 7
                },
                new ServiceCategory
                {
                    ServiceId = 3,
                    Name = "WordPress: Hosting 1st Site",
                    Costs = "450.0m",
                    Description = "$350 - Maintenance; $100 - Analytics Service",
                    IsActive = false,
                    UOM = "Month",
                    ServiceOwner = "Cormack, Garrett CITZ:EX",
                    BusAreaId = 3
                },
                new ServiceCategory
                {
                    ServiceId = 4,
                    Name = "WordPress: Hosting additional sites",
                    Costs = "400.0m",
                    Description = "$350 - Maintenance; $50 - Analytics Service; (more details in description)",
                    IsActive = false,
                    UOM = "Month",
                    ServiceOwner = "Cormack, Garrett CITZ:EX",
                    BusAreaId = 3
                },
                new ServiceCategory
                {
                    ServiceId = 5,
                    Name = "WordPress: Expert help",
                    Costs = null,
                    Description = "Minimum 1 hour",
                    IsActive = false,
                    UOM = "Hr",
                    ServiceOwner = null,
                    BusAreaId = 3
                }
            };

            var mockRepository = new Mock<IServiceCategoryRepository>();
            mockRepository.Setup(repo => repo.GetAll()).Returns(serviceCategories.OrderBy(s => s.Name));
            mockRepository.Setup(repo => repo.GetById(It.IsAny<int?>())).Returns((int? id) => serviceCategories.FirstOrDefault(s => s.ServiceId == id));
            mockRepository.Setup(repo => repo.Search(It.IsAny<string>())).Returns((string queryString) => serviceCategories.Where(s => s.Name == queryString));

            return mockRepository;
        }

        public static Mock<IClientAccountRepository> GetClientAccountRepository()
        {
            var clientAccounts = new List<ClientAccount>
{
            new ClientAccount
            {
                Id = 500,
                Name = "GCPE Marketing",
                ClientNumber = 22,
                ResponsibilityCentre = "32G31",
                ServiceLine = 34420,
                STOB = 6301,
                Project = "3200000",
                Approver = "Liu, Angela GCPE:EX",
                FinancialContact = "Liu, Angela GCPE:EX",
                ExpenseAuthorityName = "Liu, Angela GCPE:EX",
                PrimaryContact = "Liu, Angela GCPE:EX",
                ServicesEnabled = "Search; Video Hosting; Other",
                Notes = "Responsibility centre updated at request of Josh St. Gelais, Feb. 6 / Carolyn Mellor",
                OrganizationId = 36,
                IsActive = false,
            },
            new ClientAccount
            {
                Id = 501,
                Name = "IT Services",
                ClientNumber = 23,
                ResponsibilityCentre = "45G12",
                ServiceLine = 11234,
                STOB = 6302,
                Project = "4200001",
                Approver = "Smith, John CITZ:EX",
                FinancialContact = "Smith, John CITZ:EX",
                ExpenseAuthorityName = "Smith, John CITZ:EX",
                PrimaryContact = "Doe, Jane CITZ:EX",
                ServicesEnabled = "Web Hosting; IT Support; Security",
                Notes = "Project updated March 1 by request of Jane Doe",
                OrganizationId = 42,
                IsActive = true
            },
            new ClientAccount
            {
                Id = 502,
                Name = "Communications Branch",
                ClientNumber = 24,
                ResponsibilityCentre = "56B44",
                ServiceLine = 22345,
                STOB = 6303,
                Project = "5300002",
                Approver = "Jones, Emily GCPE:EX",
                FinancialContact = "Jones, Emily GCPE:EX",
                ExpenseAuthorityName = "Jones, Emily GCPE:EX",
                PrimaryContact = "Miller, Sam GCPE:EX",
                ServicesEnabled = "Media Monitoring; Communications Strategy",
                Notes = "New project assigned April 15 by Sam Miller",
                OrganizationId = 48,
                IsActive= true,
            },
            new ClientAccount
            {
                Id = 503,
                Name = "Finance and Treasury",
                ClientNumber = 25,
                ResponsibilityCentre = "78F21",
                ServiceLine = 33456,
                STOB = 6304,
                Project = "6400003",
                Approver = "Taylor, Chris FIN:EX",
                FinancialContact = "Taylor, Chris FIN:EX",
                ExpenseAuthorityName = "Taylor, Chris FIN:EX",
                PrimaryContact = "Walker, Alice FIN:EX",
                ServicesEnabled = "Financial Reporting; Budget Planning",
                Notes = "Budget planning service added July 20",
                OrganizationId = 50,
                IsActive = true
            }
            };

            var mockRepository = new Mock<IClientAccountRepository>();
            mockRepository.Setup(repo => repo.GetAll()).Returns(clientAccounts.OrderBy(c => c.Name));
            mockRepository.Setup(repo => repo.GetClientAccount(It.IsAny<int>())).Returns((int accountId) => clientAccounts.FirstOrDefault(c => c.Id == accountId));
            mockRepository.Setup(repo => repo.SearchClientAccounts(It.IsAny<string>())).Returns((string queryString) => clientAccounts.Where(c => c.Name.Contains(queryString)).OrderBy(c => c.Name));
            mockRepository.Setup(repo => repo.GetClientIdFromClientNumber(It.IsAny<int>())).Returns((int clientNumber) => clientAccounts.FirstOrDefault(x => x.ClientNumber == clientNumber)?.Id ?? 0);
            mockRepository.Setup(repo => repo.GetAccountsByContactName(It.IsAny<string>())).Returns((string contactName) =>
            {
                var userAccounts = clientAccounts.Where(x => x.PrimaryContact == contactName).ToList();
                userAccounts.AddRange(clientAccounts.Where(x => x.FinancialContact == contactName));
                userAccounts.AddRange(clientAccounts.Where(x => x.Approver == contactName));
                return userAccounts.Distinct();
            });
            mockRepository.Setup(repo => repo.GetInactiveAccounts()).Returns(clientAccounts.Where(x => !x.IsActive));
            mockRepository.Setup(repo => repo.GetAccountsByOrgId(It.IsAny<int>())).Returns((int orgId) => clientAccounts.Where(x => x.OrganizationId == orgId));

            return mockRepository;
        }

        public static Mock<IMinistryRepository> GetMinistryRepository()
        {
            var ministries = new List<Ministry>
            {
                new Ministry { Id = 1, Title = "Energy, Mines and Low Carbon Innovation", Acronym = "EMLI" },
                new Ministry { Id = 2, Title = "Tourism, Arts, Culture and Sport", Acronym = "TACS" },
                new Ministry { Id = 3, Title = "Agency, Board or Commission", Acronym = "ABC" },
                new Ministry { Id = 4, Title = "Agriculture and Food", Acronym = "AF" },
                new Ministry { Id = 5, Title = "Attorney General", Acronym = "AG" }

            };

            var mockRepository = new Mock<IMinistryRepository>();
            mockRepository.Setup(repo => repo.GetAll()).Returns(ministries.OrderBy(m => m.Acronym));
            mockRepository.Setup(repo => repo.GetById(It.IsAny<int>())).Returns((int id) => ministries.FirstOrDefault(m => m.Id == id));

            return mockRepository;
        }

        public static Mock<IFiscalHistoryRepository> GetFiscalHistoryRepository()
        {
            var fiscalHistories = new List<FiscalHistory>
            {
                new FiscalHistory(11612, 259, 400, 3) { Id = 4244 },
                new FiscalHistory(11664, 259, 400, 3) { Id = 4245 },
                new FiscalHistory(11667, 259, 400, 3) { Id = 4246 },
                new FiscalHistory(11668, 259, 400, 3) { Id = 4247 },
                new FiscalHistory(11671, 259, 400, 3) { Id = 4248 },
                new FiscalHistory(11672, 259, 400, 3) { Id = 4249 },
            }.AsQueryable();

            var mockRepository = new Mock<IFiscalHistoryRepository>();
            mockRepository.Setup(repo => repo.All).Returns(fiscalHistories);
            mockRepository.Setup(repo => repo.GetFiscalHistoriesByChargeId(It.IsAny<int>()))
                .Returns((int id) => fiscalHistories.Where(x => x.BillId == id));
            mockRepository.Setup(repo => repo.GetFiscalHistoryByFiscalPeriodId(It.IsAny<int>()))
                .Returns((int id) => fiscalHistories.Where(x => x.PeriodId == id));
            mockRepository.Setup(repo => repo.GetFiscalHistoryById(It.IsAny<int>()))
                .Returns((int id) => fiscalHistories.FirstOrDefault(x => x.Id == id));

            return mockRepository;
        }

        public static Mock<IBillRepository> GetBillRepository()
        {
            var bills = new List<Bill>
            {
                new Bill
                {
                    Id = 1,
                    ServiceCategoryId = 101,
                    Amount = 400,
                    Quantity = 3,
                    IsActive = true,
                    EndDate = null, // ongoing bill
                    CurrentFiscalPeriodId = 0,
                    ServiceCategory = new ServiceCategory
                    {
                        ServiceId = 101,
                        Name = "Service A",
                        Costs = "400.0",
                        UOM = "Month",
                        IsActive = true
                    },
                    MostRecentActiveFiscalPeriod = new FiscalPeriod
                    {
                        Id = 259,
                        Period = "Fiscal 23/24 Quarter 3"
                    }
                },
                new Bill
                {
                    Id = 2,
                    ServiceCategoryId = 102,
                    Amount = 500,
                    Quantity = 3,
                    IsActive = true,
                    EndDate = null, // ongoing bill
                    CurrentFiscalPeriodId = 0,
                    ServiceCategory = new ServiceCategory
                    {
                        ServiceId = 102,
                        Name = "Service B",
                        Costs = "500.0",
                        UOM = "Month",
                        IsActive = true
                    },
                    MostRecentActiveFiscalPeriod = new FiscalPeriod
                    {
                        Id = 259,
                        Period = "Fiscal 23/24 Quarter 3"
                    }
                },
                new Bill
                {
                    Id = 3,
                    ServiceCategoryId = 103,
                    Amount = 600,
                    Quantity = 3,
                    IsActive = true,
                    EndDate = new DateTime(2024, 5, 31), // finite bill ending
                    CurrentFiscalPeriodId = 0,
                    ServiceCategory = new ServiceCategory
                    {
                        ServiceId = 103,
                        Name = "Service C",
                        Costs = "600.0",
                        UOM = "Month",
                        IsActive = true
                    },
                    MostRecentActiveFiscalPeriod = new FiscalPeriod
                    {
                        Id = 259,
                        Period = "Fiscal 23/24 Quarter 3"
                    }
                }
            };

            var mockRepository = new Mock<IBillRepository>();

            mockRepository.Setup(repo => repo.AllBills).Returns(bills);

            // Setup mock to allow tracking changes to bills
            mockRepository.Setup(repo => repo.Update(It.IsAny<Bill>()))
                .Callback((Bill bill) =>
                {
                    var existingBill = bills.FirstOrDefault(b => b.Id == bill.Id);
                    if (existingBill != null)
                    {
                        existingBill.CurrentFiscalPeriodId = bill.CurrentFiscalPeriodId;
                        existingBill.Quantity = bill.Quantity;
                        existingBill.Amount = bill.Amount;
                    }
                });

            return mockRepository;
        }


    }
}
