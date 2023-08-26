using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceBilling.BillingManagement.UI.Models;
using ServiceBilling.BillingManagement.UI.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Graph;
using Microsoft.Identity.Web;

namespace ServiceBilling.BillingManagement.UI.Controllers
{
    public class ClientAccountController: Controller
    {
        private readonly GraphServiceClient _graphServiceClient;
        private readonly MicrosoftIdentityConsentAndConditionalAccessHandler _consentHandler;

        public ClientAccountController(IConfiguration configuration,
                            GraphServiceClient graphServiceClient,
                            MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
        {
            _graphServiceClient = graphServiceClient;
            _consentHandler = consentHandler;

        }
    }
}
