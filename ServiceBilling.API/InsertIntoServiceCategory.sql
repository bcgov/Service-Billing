
INSERT INTO [(localdb)\MSSQLLocalDB.aspnet-WebApplication2-0441b489-9723-4770-814a-c630fcefdc34].dbo.ServiceCategories
(ServiceCategoryId, GdxBusinessArea, [Name], Costs, [Description], UOM)
Select ServiceId, GDXBusArea, [Name], Costs, [Description], UOM 
From Billing.dbo.ServiceCategories;
