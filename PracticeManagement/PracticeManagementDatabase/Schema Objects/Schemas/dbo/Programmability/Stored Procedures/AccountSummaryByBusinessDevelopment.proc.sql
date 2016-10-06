﻿CREATE PROCEDURE [dbo].[AccountSummaryByBusinessDevelopment]
(
	@AccountId	INT,
	@BusinessUnitIds	NVARCHAR(MAX) = NULL,
	@StartDate	DATETIME,
	@EndDate	DATETIME
)
AS
BEGIN
	
	DECLARE @StartDateLocal DATETIME ,
		@EndDateLocal DATETIME,
		@AccountIdLocal INT,
		@BusinessUnitIdsLocal	NVARCHAR(MAX),
		@FutureDate DATETIME

	SELECT @StartDateLocal = CONVERT(DATE, @StartDate)
		 , @EndDateLocal = CONVERT(DATE, @EndDate)
		 , @AccountIdLocal = @AccountId
		 , @BusinessUnitIdsLocal = @BusinessUnitIds
		 , @FutureDate = dbo.GetFutureDate()

	SELECT PG.GroupId AS [BusinessUnitId]
		 , PG.Name AS [BusinessUnitName]
		 , PG.Code AS [GroupCode]
		 , PG.Active
		 , TT.Name AS [TimeTypeName]
		 , TT.Code AS [TimeTypeCode]
		 , P.PersonId
		 , P.EmployeeNumber
		 , ISNULL(P.PreferredFirstName,P.FirstName) AS FirstName
		 , P.LastName
		 , TE.ChargeCodeDate
		 , TEH.ActualHours AS NonBillableHours
		 , TE.Note
	FROM
		dbo.TimeEntry TE
		INNER JOIN dbo.TimeEntryHours TEH
			ON TEH.TimeEntryId = TE.TimeEntryId AND TE.ChargeCodeDate BETWEEN @StartDateLocal AND @EndDateLocal
		INNER JOIN dbo.ChargeCode CC
			ON CC.Id = TE.ChargeCodeId 
		INNER JOIN dbo.ProjectGroup PG
			ON PG.GroupId = CC.ProjectGroupId
		INNER JOIN dbo.TimeType AS TT
			ON TT.TimeTypeId = CC.TimeTypeId
		INNER JOIN dbo.Person P
			ON P.PersonId = TE.PersonId AND TE.ChargeCodeDate <= ISNULL(P.TerminationDate, @FutureDate)
		INNER JOIN dbo.Project Pro
		    ON Pro.ProjectId = CC.ProjectId

	WHERE
		CC.ClientId = @AccountIdLocal
		AND (@BusinessUnitIdsLocal IS NULL
					   OR PG.GroupId IN (SELECT ResultId
						  FROM
					      dbo.ConvertStringListIntoTable(@BusinessUnitIdsLocal)))
	    AND(CC.TimeEntrySectionId = 2 OR Pro.IsBusinessDevelopment = 1) -- Added this condition as part of PP29 changes by Nick.
	ORDER BY
		P.LastName
	  , P.FirstName
	  , TE.ChargeCodeDate
	  , TT.Name

 END

