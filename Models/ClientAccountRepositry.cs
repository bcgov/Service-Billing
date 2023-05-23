﻿using Microsoft.EntityFrameworkCore;
using Service_Billing.Data;

namespace Service_Billing.Models
{
    public class ClientAccountRepositry : IClientAccountRepository
    {
        private readonly ServiceBillingContext _context;
        public ClientAccountRepositry(ServiceBillingContext context)
        {
            _context = context;
        }

        public void CreateClientAccount(ClientAccount account)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ClientAccount> GetAll()
        {
            return _context.clientAccounts.OrderBy(c => c.clientName);
        }

        public ClientAccount? GetClientAccount(int accountId)
        {
            return _context.clientAccounts.FirstOrDefault(c => c.accountId == accountId);
        }

        public IEnumerable<ClientAccount> SearchClientAccounts(string queryString)
        {
            return _context.clientAccounts.Include(c => c.clientName.Contains(queryString));
        }
    }
}
