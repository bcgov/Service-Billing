insert into ServiceCategories
(GdxBusinessArea, [Name], Costs, [Description], UOM)
Select GDXBusArea, [Name], Costs, [Description], UOM 
from [NH500919].[Billing].[dbo].[ServiceCategories];

/* Had to make modifications to receiving table: id gets default value of NEWID(), Dates get default value of GETDATE() */