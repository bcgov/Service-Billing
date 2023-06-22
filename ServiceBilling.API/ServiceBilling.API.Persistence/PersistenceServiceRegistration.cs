using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceBilling.API.Application.Contracts;
using ServiceBilling.API.Application.Contracts.Persistence;
using ServiceBilling.API.Persistence.Repositories;

namespace ServiceBilling.API.Persistence
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnectionString")));

            services.AddScoped(typeof(IAsyncRepository<>), typeof(BaseRepository<>));

            services.AddScoped<IClientAccountRepository, ClientAccountRepository>();
            services.AddScoped<IClientTeamRepository, ClientTeamRepository>();
            // services.AddScoped<IEventRepository, EventRepository>();
            // services.AddScoped<IOrderRepository, OrderRepository>();

            return services;
        }
    }
}
