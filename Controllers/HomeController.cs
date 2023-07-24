using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Service_Billing.Models;

using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
//using Microsoft.Graph.Models;
using Microsoft.Identity.Client;

using System;
using System.IO;
using System.Threading.Tasks;


namespace Service_Billing.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly GraphServiceClient _graphServiceClient;

    private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;

    private string[] _graphScopes = { "user.read" };

    public HomeController(ILogger<HomeController> logger,
          IConfiguration configuration,
                            GraphServiceClient graphServiceClient,
                            MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
    {
        _logger = logger;
        _graphServiceClient = graphServiceClient;
        _consentHandler = consentHandler;

        // Capture the Scopes for Graph that were used in the original request for an Access token (AT) for MS Graph as
        // they'd be needed again when requesting a fresh AT for Graph during claims challenge processing
    //    _graphScopes = configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ');
    }

    [Authorize(Roles = "GDXBillingService.FinancialOfficer, GDXBillingService.Owner, GDXBillingService.User")]
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

   
}
