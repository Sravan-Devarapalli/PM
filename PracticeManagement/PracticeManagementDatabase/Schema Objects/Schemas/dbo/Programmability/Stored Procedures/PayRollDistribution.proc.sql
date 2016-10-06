﻿--Create this sproc as the client requires to run the report it manually.Therefore due to formatting changes created it(refer to defect #3151).
--If there is any change in the [PersonTimeEntriesDetails] SPROC for payroll distribution functionality change this also.
CREATE PROCEDURE [dbo].[PayRollDistribution]
(
	@PersonId INT = NULL,
	@StartDate DATETIME ,
	@EndDate DATETIME
)
AS 
BEGIN

	SET NOCOUNT ON;

	DECLARE @StartDateLocal DATETIME ,
		@EndDateLocal DATETIME ,
		@PersonIdLocal INT ,
		@HolidayTimeType INT ,
		@FutureDate DATETIME

	SELECT @StartDateLocal = CONVERT(DATE, @StartDate), @EndDateLocal = CONVERT(DATE, @EndDate),@PersonIdLocal = @PersonId,@HolidayTimeType = dbo.GetHolidayTimeTypeId(),@FutureDate = dbo.GetFutureDate()

	;WITH    PersonDayWiseByProjectsBillableTypes
				AS ( SELECT   M.ProjectId ,
							MP.PersonId,
							C.Date ,
							MIN(CAST(M.IsHourlyAmount AS INT)) MinimumValue ,
							MAX(CAST(M.IsHourlyAmount AS INT)) MaximumValue
							,CASE WHEN MAX(CAST(m.IsHOurlyAmount AS INT)) = 1 AND MIN(CAST(m.IsHourlyAmount AS INT)) = 1 THEN SUM(MPE.Amount * MPE.HoursPerDay)/ SUM(MPE.HoursPerDay)
							ELSE NULL END HourlyRate
					FROM     dbo.MilestonePersonEntry AS MPE
							INNER JOIN dbo.MilestonePerson AS MP ON MP.MilestonePersonId = MPE.MilestonePersonId
							INNER JOIN dbo.Milestone AS M ON M.MilestoneId = MP.MilestoneId
							INNER JOIN dbo.Calendar AS C ON C.Date BETWEEN MPE.StartDate AND MPE.EndDate
															AND C.Date BETWEEN @StartDateLocal AND @EndDateLocal
					WHERE    (@PersonIdLocal IS NULL OR MP.PersonId = @PersonIdLocal)
							AND M.StartDate < @EndDateLocal
							AND @StartDateLocal < M.ProjectedDeliveryDate
					GROUP BY M.ProjectId ,
							MP.PersonId,
							C.Date
					)

		SELECT  P.EmployeeNumber AS [Employee Id], 
				P.FirstName +', ' + P.LastName AS [Employee],
				ISNULL(Pa.TimescaleName,'') AS [Pay Type],
				CASE WHEN P.IsOffshore = 1 THEN 'Yes' ELSE 'No' END AS [Is Offshore?],
				C.Code AS [Account] ,
				C.Name AS [Account Name] ,
				BU.Code AS [Business Unit] ,
				BU.Name AS [Business Unit Name] ,
				PRO.ProjectNumber AS Project,
				PRO.Name AS [Project Name],
				(CASE WHEN ( CC.TimeEntrySectionId <> 1 ) THEN '' ELSE PS.Name END ) AS [Status],
				(CASE	WHEN ( PDBR.MinimumValue IS NULL OR CC.TimeEntrySectionId <> 1 ) 
						THEN ''
						WHEN ( PDBR.MinimumValue = PDBR.MaximumValue AND PDBR.MinimumValue = 0 )
						THEN 'Fixed'
						WHEN ( PDBR.MinimumValue = PDBR.MaximumValue AND PDBR.MinimumValue = 1)
						THEN 'Hourly'
				ELSE 'Both'
				END) AS [Billing] ,
				'01' AS Phase,
				TT.Code AS [Work Type] ,
				TT.Name AS [Work Type Name] ,
				CONVERT(DATE,TE.ChargeCodeDate)  AS [Date],
				ROUND(SUM(CASE WHEN TEH.IsChargeable = 1
								THEN TEH.ActualHours
								ELSE 0
							END), 2)  AS [Billable Hours],
				ROUND(SUM(CASE WHEN TEH.IsChargeable = 0
								THEN TEH.ActualHours
								ELSE 0
							END), 2) AS [Non-Billable Hours],
				ROUND(SUM(TEH.ActualHours), 2) AS [Total Hours],
				ISNULL(CONVERT(NVARCHAR(100),CASE WHEN CC.TimeEntrySectionId = 3 THEN NULL ELSE CONVERT(numeric(36,2),PDBR.HourlyRate) END),'') AS [Bill Rate],
				ISNULL(CONVERT(NVARCHAR(100),CONVERT(numeric(36,2),Pa.AmountHourly)),'') AS [Pay Rate],
				(CASE WHEN CC.TimeEntrySectionId = 4 THEN TE.Note + dbo.GetApprovedByName(TE.ChargeCodeDate, TT.TimeTypeId, @PersonIdLocal)
						ELSE TE.Note
					END ) AS Note
		FROM    dbo.TimeEntry AS TE
				INNER JOIN dbo.TimeEntryHours AS TEH ON TEH.TimeEntryId = TE.TimeEntryId
				INNER JOIN dbo.ChargeCode CC ON CC.Id = TE.ChargeCodeId
				INNER JOIN dbo.ProjectGroup BU ON BU.GroupId = CC.ProjectGroupId
				INNER JOIN dbo.Client C ON CC.ClientId = C.ClientId
				INNER JOIN dbo.Project PRO ON PRO.ProjectId = CC.ProjectId
				INNER JOIN dbo.ProjectStatus PS ON PRO.ProjectStatusId = PS.ProjectStatusId
				INNER JOIN dbo.TimeType TT ON TT.TimeTypeId = CC.TimeTypeId
				INNER JOIN dbo.PersonStatusHistory PTSH ON TE.ChargeCodeDate BETWEEN PTSH.StartDate
															AND ISNULL(PTSH.EndDate,@FutureDate) AND PTSH.PersonId = TE.PersonId
				INNER JOIN dbo.Person P ON P.PersonId = TE.PersonId
				LEFT JOIN PersonDayWiseByProjectsBillableTypes PDBR ON PDBR.ProjectId = CC.ProjectId AND TE.PersonId = PDBR.PersonId
															AND PDBR.Date = TE.ChargeCodeDate
				LEFT JOIN dbo.v_Pay Pa ON Pa.PersonId = P.PersonId AND TE.ChargeCodeDate BETWEEN Pa.StartDate AND (ISNULL(Pa.EndDate, @FutureDate) -1)
		WHERE   (@PersonIdLocal IS NULL OR TE.PersonId = @PersonIdLocal)
				AND TE.ChargeCodeDate BETWEEN @StartDateLocal
										AND     @EndDateLocal
				AND ( CC.timeTypeId != @HolidayTimeType
						OR ( CC.timeTypeId = @HolidayTimeType
							AND PTSH.PersonStatusId IN (1,5)
							)
					)
		GROUP BY TE.PersonId ,
				P.EmployeeNumber,
				P.FirstName,
				P.LastName,
				P.IsOffshore,
				CC.TimeEntrySectionId ,
				C.ClientId ,
				C.Name ,
				C.Code ,
				BU.Name ,
				BU.Code ,
				PRO.ProjectId ,
				PRO.Name ,
				PRO.ProjectNumber ,
				PS.Name ,
				TT.TimeTypeId ,
				TT.Name ,
				TT.Code ,
				TE.ChargeCodeDate ,
				TE.Note ,
				PDBR.MinimumValue ,
				PDBR.MaximumValue,
				PDBR.HourlyRate,
				Pa.AmountHourly,
				Pa.TimescaleName
		ORDER BY P.FirstName,
				P.LastName,
				CC.TimeEntrySectionId ,
				PRO.ProjectNumber ,
				TE.ChargeCodeDate ,
				TT.Name
END	
	

