IF EXISTS (SELECT 1 
	FROM [dbo].[MyCompany] c
	JOIN [dbo].[MyCompanyAddresses] ca ON ca.MyCompanyID = c.MyCompanyID
	JOIN [dbo].[Addresses] a ON ca.AddressID = a.AddressID
	WHERE a.City = 'Варна')
BEGIN
				
	UPDATE [dbo].[Calendar] SET [EnumCalendarDayTypeID] = 2
	WHERE Date = '2024-08-15 00:00:00.000'

	SELECT DB_Name() as [Database], c.Name as Company, a.City as City,
	(SELECT EnumCalendarDayTypeID FROM [dbo].[Calendar] WHERE Date = '2024-08-15 00:00:00.000') as WorkingDayType
	FROM [dbo].[MyCompany] c
	JOIN [dbo].[MyCompanyAddresses] ca ON ca.MyCompanyID = c.MyCompanyID
	JOIN [dbo].[Addresses] a ON ca.AddressID = a.AddressID
END