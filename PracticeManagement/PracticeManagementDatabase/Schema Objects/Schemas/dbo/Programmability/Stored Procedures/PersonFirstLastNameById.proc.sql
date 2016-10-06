﻿-- =============================================
-- Updated by : Sainath.CH
-- Update Date: 05-31-2012
-- =============================================
CREATE PROCEDURE [dbo].[PersonFirstLastNameById]
	@PersonId int
AS
BEGIN
	DECLARE @NOW DATETIME ,@FutureDate DATETIME
	SELECT @NOW = dbo.GettingPMTime(GETUTCDATE()), @FutureDate = dbo.GetFutureDate()

	SELECT	P.FirstName,
			P.PreferredFirstName,
			P.LastName,
			P.IsStrawman,
			P.HireDate,
			TS.Name AS Timescale,
			P.IsOffshore,
			P.PersonStatusId,
			ISNULL(P.Alias,'') AS Alias,
			P.EmployeeNumber,
			P.SeniorityId,
			S.Name AS Seniority
	FROM dbo.Person P
	LEFT JOIN dbo.Pay PA ON PA.Person = P.PersonId 
							AND @NOW BETWEEN PA.StartDate  AND ISNULL(PA.EndDate-1,@FutureDate) 
	LEFT JOIN dbo.Timescale TS ON PA.Timescale = TS.TimescaleId
	LEFT JOIN dbo.Seniority S ON S.SeniorityId = P.SeniorityId
	WHERE PersonId = @PersonId
END
