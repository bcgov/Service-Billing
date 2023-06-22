using Microsoft.EntityFrameworkCore;
using ServiceBilling.API.Application;
using ServiceBilling.API.Persistence;

namespace ServiceBilling.BillingManagement.Api
{
    public static class StartupExtensions
    {
        public static WebApplication ConfigureServices(
        this WebApplicationBuilder builder)
        {
            //    AddSwagger(builder.Services);

            builder.Services.AddApplicationServices();
            // builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddPersistenceServices(builder.Configuration);

            // builder.Services.AddHttpContextAccessor();

            builder.Services.AddControllers();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Open", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            return builder.Build();
        }


        //public static WebApplication ConfigureServices(
        //this WebApplicationBuilder builder)
        //{
        //    AddSwagger(builder.Services);

        //    builder.Services.AddApplicationServices();
        //    builder.Services.AddInfrastructureServices(builder.Configuration);
        //    builder.Services.AddPersistenceServices(builder.Configuration);

        //    builder.Services.AddHttpContextAccessor();

        //    builder.Services.AddControllers();

        //    builder.Services.AddCors(options =>
        //    {
        //        options.AddPolicy("Open", builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
        //    });

        //    return builder.Build();

        //}

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            //    if (app.Environment.IsDevelopment())
            //    {
            //        app.UseSwagger();
            //        app.UseSwaggerUI(c =>
            //        {
            //            c.SwaggerEndpoint("/swagger/v1/swagger.json", "GloboTicket Ticket Management API");
            //        });
            //    }

            app.UseHttpsRedirection();

            app.UseRouting();

            //    app.UseAuthentication();

            //    app.UseCustomExceptionHandler();

            app.UseCors("Open");

            //    app.UseAuthorization();

            app.MapControllers();

            return app;
        }

        //public static WebApplication ConfigurePipeline(this WebApplication app)
        //{

        //    if (app.Environment.IsDevelopment())
        //    {
        //        app.UseSwagger();
        //        app.UseSwaggerUI(c =>
        //        {
        //            c.SwaggerEndpoint("/swagger/v1/swagger.json", "GloboTicket Ticket Management API");
        //        });
        //    }

        //    app.UseHttpsRedirection();

        //    //app.UseRouting();

        //    app.UseAuthentication();

        //    app.UseCustomExceptionHandler();

        //    app.UseCors("Open");

        //    app.UseAuthorization();

        //    app.MapControllers();

        //    return app;

        //}
        //private static void AddSwagger(IServiceCollection services)
        //{
        //    services.AddSwaggerGen(c =>
        //    {


        //        c.SwaggerDoc("v1", new OpenApiInfo
        //        {
        //            Version = "v1",
        //            Title = "GloboTicket Ticket Management API",

        //        });

        //        c.OperationFilter<FileResultContentTypeOperationFilter>();
        //    });
        //}

        public static async Task ResetDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            try
            {
                var context = scope.ServiceProvider.GetService<DataContext>();
                if (context != null)
                {
                    await context.Database.EnsureDeletedAsync();
                    await context.Database.MigrateAsync();
                }
            }
            catch (Exception ex)
            {
                //var logger = scope.ServiceProvider.GetRequiredService<ILogger>();
                //logger.LogError(ex, "An error occurred while migrating the database.");
            }
        }
    }
}
