namespace ServiceBilling.BillingManagement.UI.Models.Repositories
{
    public class MinistryRepository : IMinistryRepository
    {
        private readonly DataContext _dataContext;

        public MinistryRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        public IEnumerable<Ministry> GetAll()
        {
            return _dataContext.Ministries.OrderBy(x => x.Acronym);
        }
    }
}
