using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceBilling.API.Application.Contracts.Infrastructure;
using ServiceBilling.API.Application.Models.Mail;
using ServiceBilling.API.Infrastructure.FileExport;
using ServiceBilling.API.Infrastructure.Mail;

namespace ServiceBilling.API.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings")); //configured in API project

            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<ICsvExporter, CsvExporter>();

            return services;
        }
    }
}
