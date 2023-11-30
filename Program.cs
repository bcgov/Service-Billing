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
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using Service_Billing.HostedServices;
using static Service_Billing.HostedServices.ChargePromotionService;
using Microsoft.Graph;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Net;
using System.Net.Http.Headers;
using Service_Billing.Services.Email;
using Service_Billing.Filters;
using Microsoft.AspNetCore.Mvc.Filters;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);
string[] initialScopes = builder.Configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');

builder.Services.AddHealthChecks();

// Add services to the container.
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    //.AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.Bind("AzureAd", options);

        // This causes the signin to prompt the user for which
        // account to use - useful when there are multiple accounts signed
        // into the browser
      
        options.Events.OnTokenValidated = async context =>
        {
            var tokenAcquisition = context.HttpContext.RequestServices
                .GetRequiredService<ITokenAcquisition>();
            string[] scopes = { "user.read", "user.readbasic.all" };
            var graphClient = new GraphServiceClient(
                new DelegateAuthenticationProvider(async (request) =>
                {
                    var token = await tokenAcquisition
                        .GetAccessTokenForUserAsync(scopes, user: context.Principal);
                    request.Headers.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                })
            );
        };

        options.Events.OnAuthenticationFailed = context =>
        {
            var error = WebUtility.UrlEncode(context.Exception.Message);
            context.Response
                .Redirect($"/Home/ErrorWithMessage?message=Authentication+error&debug={error}");
            context.HandleResponse();

            return Task.FromResult(0);
        };

        options.Events.OnRemoteFailure = context =>
        {
            if (context.Failure is OpenIdConnectProtocolException)
            {
                var error = WebUtility.UrlEncode(context.Failure.Message);
                context.Response
                    .Redirect($"/Home/ErrorWithMessage?message=Sign+in+error&debug={error}");
                context.HandleResponse();
            }

            return Task.FromResult(0);
        };
    })
    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
    .AddMicrosoftGraph(builder.Configuration.GetSection("DownstreamAPI"))
    .AddInMemoryTokenCaches();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent
    // for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
    // Handling SameSite cookie according to
    options.HandleSameSiteCookieCompatibility();
});


builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
}).AddMicrosoftIdentityUI();

builder.Services.AddScoped<IBillRepository, BillRepository>();
builder.Services.AddScoped<IServiceCategoryRepository, ServiceCategoryRepository>();
builder.Services.AddScoped<IClientAccountRepository, ClientAccountRepositry>();
builder.Services.AddScoped<IClientTeamRepository, ClientTeamRepository>();
builder.Services.AddScoped<IMinistryRepository, MinistryRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();

//builder.Services.AddHostedService<ChargePromotionService>();
builder.Services.AddScoped<IScopedProcessingService, ScopedProcessingService>();

builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

builder.Services.AddScoped(provider =>
{
    return new GroupAuthorizeActionFilter();
});

builder.Services.AddMvc();

//database connection
builder.Services.AddDbContext<ServiceBillingContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ServiceBillingContext") ?? throw new InvalidOperationException("Connection string 'Service_BillingContext' not found.")));

builder.Services.AddRazorPages().AddMvcOptions(options =>
{
    var policy = new AuthorizationPolicyBuilder()
                  .RequireAuthenticatedUser()
                  .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
}).AddMicrosoftIdentityUI();

builder.Services
    .Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
    {
        options.SaveTokens = true;
        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProvider = async ctxt =>
            {
                ctxt.ProtocolMessage.RedirectUri = builder.Configuration["RuntimeAdRedirectUri"];
                await Task.Yield();
            }
        };
    });

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
}).AddMicrosoftIdentityConsentHandler();

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

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ServiceBillingContext>();
    await context.Database.MigrateAsync();
    //DbInitializer.SeedMinistries(context);
    //DbInitializer.SeedServices(context);
    //DbInitializer.SeedTeams(context);
    //DbInitializer.SeedAccounts(context);
    //DbInitializer.SeedCharges(context);
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.MapHealthChecks("/healthz");

app.Run();
