﻿CREATE PROCEDURE [dbo].[GetPTOReportForDisplay]
(
	@StartDate				DATETIME,
	@EndDate				DATETIME
)
AS
BEGIN
	DECLARE @HolidayTimeType INT
	SELECT @HolidayTimeType = dbo.GetHolidayTimeTypeId()
	;WITH PersonPayToday
	AS 
	(
		SELECT p.Person,p.StartDate,p.Timescale
		FROM dbo.Pay AS p
		INNER JOIN dbo.Person per ON per.PersonId = p.Person AND per.IsStrawman = 0
		WHERE  GETDATE() >= p.StartDate
				AND GETDATE() < p.EndDate
				AND p.Timescale = 2 -- W2-Salary
	),
	PersonPTO
	AS
	(
		SELECT PC.PersonId,
			   MIN(PC.Date) AS StartDate,
			   MAX(PC.Date) AS EndDate,
			   ISNULL(MAX(PC.TimeTypeId),@HolidayTimeType) AS TimeTypeId,
			   MAX(CONVERT(INT,PC.DayOff)) AS DayOff,
			   MAX(CONVERT(INT,PC.CompanyDayOff)) AS CompanyDayOff
		FROM v_PersonCalendar PC
		INNER JOIN PersonPayToday PPT ON PPT.Person = PC.PersonId
		WHERE PC.Date BETWEEN @StartDate AND @EndDate
			  AND PC.DayOff = 1
			  AND DATEPART(DW,PC.Date) <> 1 AND DATEPART(DW,PC.Date) <> 7
		GROUP BY PC.PersonId,CASE WHEN PC.SeriesId IS NULL THEN PC.Date ELSE PC.SeriesId END
	)
	SELECT p.EmployeeNumber AS [Person ID],
		   P.LastName+', '+P.FirstName AS [Person Name],
		   CASE WHEN PPT.DayOff = 1 AND PPT.CompanyDayOff = 0 AND PPT.TimeTypeId = @HolidayTimeType THEN 'SUB HOL' ELSE TT.Acronym END AS TimeTypeName,
		   PPT.StartDate AS [Time Off Start Date],
		   PPT.EndDate AS [Time Off End Date],
		   pro.ProjectNumber AS [Project Number],
		   pro.Name AS [Project Name],
		   PS.Name AS [Project Status],
		   c.Name AS Account,
		   BG.Name AS [Business Group],
		   PG.Name AS [Business Unit],
		   pra.Name AS [Practice Area],
		   dbo.GetProjectManagerNames(pro.ProjectId) AS [Project Access],
		   smanager.LastName+', '+smanager.FirstName AS [Engagement Manager],
		   directorId.LastName+', '+directorId.FirstName AS [Executive in Charge] 
	FROM PersonPTO PPT 
	LEFT JOIN v_MilestonePerson VMP ON PPT.PersonId = VMP.PersonId 
									AND VMP.StartDate <= PPT.EndDate AND PPT.StartDate <= VMP.EndDate 
									AND VMP.ProjectNumber <> 'P031000'
									AND VMP.ProjectStatusId IN (2,3,4)
	INNER JOIN dbo.Person P ON P.PersonId = PPT.PersonId
	INNER JOIN dbo.TimeType TT ON TT.TimeTypeId = PPT.TimeTypeId
	LEFT JOIN dbo.Project pro ON pro.ProjectId = VMP.ProjectId 
	LEFT JOIN dbo.ProjectStatus PS ON PS.ProjectStatusId = pro.ProjectStatusId
	LEFT JOIN dbo.ProjectGroup PG ON PG.GroupId = pro.GroupId	
	LEFT JOIN dbo.BusinessGroup BG ON BG.BusinessGroupId = PG.BusinessGroupId
	LEFT JOIN dbo.Practice pra ON pra.PracticeId = pro.PracticeId
	LEFT JOIN dbo.Person smanager ON smanager.PersonId = pro.EngagementManagerId
	LEFT JOIN dbo.Person directorId ON directorId.PersonId = pro.ExecutiveInChargeId
	LEFT JOIN dbo.Client C ON C.ClientId = pro.ClientId
	GROUP BY   PPT.PersonId,
			   p.EmployeeNumber,
			   P.FirstName,
			   P.LastName,
			   PPT.TimeTypeId,
			   TT.Acronym,
			   PPT.StartDate,
			   PPT.EndDate,
			   pro.ProjectNumber,
			   pro.Name,
			   pro.ProjectStatusId,
			   PS.Name,
			   pro.ClientId,
			   C.Name,
			   pro.GroupId,
			   PG.Name,
			   PG.BusinessGroupId,
			   BG.Name,
			   pro.PracticeId,
			   pra.Name,
			   pro.ProjectId,
			   pro.EngagementManagerId,
			   smanager.LastName+', '+smanager.FirstName,
			   pro.ExecutiveInChargeId,
			   directorId.LastName,
			   directorId.FirstName,
			   PPT.DayOff,
			   PPT.CompanyDayOff
	ORDER BY P.LastName,p.FirstName,PPT.StartDate
END
