﻿CREATE PROCEDURE [dbo].[GetCompanyName]
AS
BEGIN
SET NOCOUNT ON;
	SELECT TOP(1) Title 
	FROM CompanyLogoSetting
END
GO
