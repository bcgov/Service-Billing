using ServiceBilling.BillingManagement.Api;

using Serilog;

// TODO(al): configure logging

var builder = WebApplication.CreateBuilder(args);

var app = builder
       .ConfigureServices()
       .ConfigurePipeline();

await app.ResetDatabaseAsync();

app.Run();
