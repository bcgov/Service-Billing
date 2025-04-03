# Service-Billing
This is the GDX Service Billing application. It replaced the Sharepoint 2016 based BillEase. This application provides a place 
to enter client billing data, manage client billing data and run reports to show billing amounts for the quarterly billing cycle.
The billing services that are managed here include Analytics, WordPress and Orbeon forms. GDX administrators are able to manage 
client data and run reports, while ministry clients are provided with some self serve options to create new client accounts and 
update their own data.  

## Azure App registration
can be found here: 
- (PROD) https://portal.azure.com/#view/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/~/Overview/appId/8ece4ae7-20bb-407e-b10d-333b3d873539/isMSAApp~/false
- (DEV) https://portal.azure.com/#view/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/~/Overview/appId/5c9b5348-b1c8-4d44-bf01-53525f66972d/isMSAApp~/false
- (TEST) https://portal.azure.com/#view/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/~/Overview/appId/1638f85e-a681-4299-b6dc-e66ed2289fbe/isMSAApp~/false



## user management
Application roles include Admin, Service Owner and User, defined in the Azure App registration. 
- Admins have full access, and may view all accounts and billed services, create new accounts, charges and service categories, and run reports.
- Service Owners may view all charges for which their owned services are billed for, and make administrative changes to those services.
- Users have the most restricted access, and may view Client Accounts on which they are contacts, as well as charges associated with those accounts.

## License
Copyright 2019 Province of British Columbia

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

## Technical Overview for Developers
Service Billing is fairly standard MVC application developed in Asp.Net, using an SQL backend. Microsoft Entity Framework Core is leveraged for database transactions.
I encourage developers new to the application to familiarize themselves with the various relationships between entities and how they are mapped. 
The terms "charge" and "bill" are used interchangeably throughout the application. The only reason for this is that models were set up using "bill" before it was stated that "charge" was the preferred term.

### Change logging
Change logs, viewable in the Details views for Client Account, Charge and Service Category models was added relatively late in the development timeline before project handoff. 
The way change logs are presented in these views is rather rough, and could use some UX improvements.

### Promoting charges to a new quarter
Every new quarter, the application runs the Charge Promotion Service, which updates charge fiscal periods to the new quarter provided the charge is active, and does not have an end date earlier than the start of the new quarter.
the ChargePromotionService is a background task setup as a "hosted service". See https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services for more information about these.

### Openshift environments
https://console.apps.silver.devops.gov.bc.ca/builds/ns/baf118-dev
https://console.apps.silver.devops.gov.bc.ca/builds/ns/baf118-test
https://console.apps.silver.devops.gov.bc.ca/builds/ns/baf118-prod

### Getting help
It's always a bit of a pain to inherit someone else's code. Feel free to contact Alexander Carmichael in BCS for help. 



