﻿CREATE PROCEDURE [dbo].[GetAllDomains]
AS
BEGIN
	SELECT Name FROM Domain ORDER BY SortOrder
END

