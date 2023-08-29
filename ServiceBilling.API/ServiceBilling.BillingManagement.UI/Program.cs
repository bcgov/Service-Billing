
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.EntityFrameworkCore;
using ServiceBilling.BillingManagement.UI.Models;
using Microsoft.Extensions.DependencyInjection;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using ServiceBilling.BillingManagement.UI.Models;
using ServiceBilling.BillingManagement.UI.Models.Repositories;
using ServiceBilling.BillingManagement.UI.Services.Mail;

var builder = WebApplication.CreateBuilder(args);
string[] initialScopes = builder.Configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');

// Add services to the container.
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
    .AddMicrosoftGraph(builder.Configuration.GetSection("DownstreamAPI"))
    .AddInMemoryTokenCaches();

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

builder.Services.AddDbContext<DataContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

builder.Services.AddServerSideBlazor()
               .AddMicrosoftIdentityConsentHandler();

builder.Services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
builder.Services.AddScoped<IChargesRepository, ChargesRepository>();
builder.Services.AddScoped<IClientAccountRepository, ClientAccountRepository>();
builder.Services.AddScoped<IClientTeamRepository, ClientTeamRepository>();
builder.Services.AddScoped<IMinistryRepository, MinistryRepository>();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//role based access
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireFinancialOfficerRole",
         policy => policy.RequireRole("GDXBillingService.FinancialOfficer"));
    options.AddPolicy("RequireOwnerRole",
        policy => policy.RequireRole("GDXBillingService.Owner"));
    options.AddPolicy("RequireUserRole",
        policy => policy.RequireRole("GDXBillingService.User"));
});

//track session data
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DataContext>();
    await context.Database.MigrateAsync();
    DbInitializer.Seed(context);
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
