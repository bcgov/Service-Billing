using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Service_Billing.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Service_Billing.Models.Repositories;
using Service_Billing.Models;
using Microsoft.AspNetCore.HttpOverrides;

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

builder.Services.AddScoped<IBillRepositroy, BillRepository>();
builder.Services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
builder.Services.AddScoped<IClientAccountRepository, ClientAccountRepositry>();
builder.Services.AddScoped<IClientTeamRepository, ClientTeamRepository>();
builder.Services.AddScoped <IMinistryRepository, MinistryRepository>();

builder.Services.AddMvc();

//database connection
builder.Services.AddDbContext<ServiceBillingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ServiceBillingContext") ?? throw new InvalidOperationException("Connection string 'Service_BillingContext' not found.")));

builder.Services.AddRazorPages()
    .AddMicrosoftIdentityUI();

builder.Services.AddServerSideBlazor()
               .AddMicrosoftIdentityConsentHandler();

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

var fordwardedHeaderOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
fordwardedHeaderOptions.KnownNetworks.Clear();
fordwardedHeaderOptions.KnownProxies.Clear();

// for making Azure AD OAuth work when deployed to OpenShift. 
app.UseForwardedHeaders(fordwardedHeaderOptions);
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
