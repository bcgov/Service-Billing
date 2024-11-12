# Service-Billing
This is the GDX Service Billing application. It replaced the Sharepoint 2016 based BillEase. This application provides a place to enter client billing data, manage client billing data and run reports to show billing amounts for the quarterly billing cycle. The billing services that are managed here include Analytics, WordPress and Orbeon forms. GDX administrators are able to manage client data and run reports, while ministry clients are provided with some self serve options to create new client accounts and update their own data.  

## Azure App registration
can be found here: https://portal.azure.com/#view/Microsoft_AAD_RegisteredApps/ApplicationMenuBlade/~/Overview/appId/8ece4ae7-20bb-407e-b10d-333b3d873539/isMSAApp~/false

## user management
There are roles, Admin, Service Owner and User, defined in the Azure App registration. 
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

