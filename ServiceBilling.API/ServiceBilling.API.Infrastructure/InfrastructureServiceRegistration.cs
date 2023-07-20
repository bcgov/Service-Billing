using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceBilling.API.Application.Contracts.Infrastructure;
using ServiceBilling.API.Infrastructure.Mail;

namespace ServiceBilling.API.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IEmailService, EmailService>();
            
            return services;
        }
    }
}
