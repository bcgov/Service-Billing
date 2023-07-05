# Service-Billing
This is the GDX Service Billing application. It will be replacing the existing Sharepoint 2016 based BillEase. The new application will provide a place to enter client billing data, manage client billing data and run reports to show client billing amounts for the quarterly billing cycle. The billing services that are managed here include Analytics, WordPress and Orbeon forms. GDX administrators will be able to manage client data and run reports, while ministry clients will be provided with some "self serve" options to create new client accounts and update their own data.  

## Azure App registration
can be found here: https://portal.azure.com/#view/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/~/Overview/appId/e6008dda-22e3-43b4-acf7-a953ac99b661/isMSAApp~/false

## user management
Right now, there are roles defined in the Azure App registration. There is some rudimentary role-based access restrictions in the home controller
as well as the view itself. I've tested it, and it works. It can serve as a example as we go forward. 

We will probably switch to using Keycloak authentication, and it looks like there's some scripts to help with this. https://gcpe.visualstudio.com/Hub/_wiki/wikis/Hub.wiki/41/News-Dashboard-Hub-API-Authentication

## Integration with Jira
I don't know. We'll figure it out. I'll skeleton out some controller code. 
