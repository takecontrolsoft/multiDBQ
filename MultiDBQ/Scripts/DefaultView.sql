SELECT DB_NAME() as [Database], c.[Name] as Company, a.City as City
FROM [dbo].[MyCompany] c
JOIN [dbo].[MyCompanyAddresses] ca ON ca.MyCompanyID = c.MyCompanyID
JOIN [dbo].[Addresses] a ON ca.AddressID = a.AddressID