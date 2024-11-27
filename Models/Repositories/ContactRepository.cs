﻿using Service_Billing.Data;

namespace Service_Billing.Models.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly ServiceBillingContext _billingContext;
        private readonly ILogger<BillRepository> _logger;

        public ContactRepository(ServiceBillingContext billingContext, ILogger<BillRepository> logger)
        {
            _billingContext = billingContext;
            _logger = logger;
        }

        public async Task AddContact(Contact contact)
        {
            await _billingContext.Contacts.AddAsync(contact);
            await _billingContext.SaveChangesAsync();
        }

        public Contact Get(int id)
        {
            return _billingContext.Contacts.FirstOrDefault(x => x.Id == id);
        }

        public Contact GetByEmail(string email)
        {
            throw new NotImplementedException();
        }
    }
}