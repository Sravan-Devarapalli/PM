-- =========================================================================
-- Author:		ThulasiRam.P
-- Create date: 03-15-2012
-- Description:  Time Entries grouped by workType and Resource for a Project.
-- Updated by : Srinivas.M
-- Update Date: 09-25-2012
-- =========================================================================
CREATE PROCEDURE [dbo].[ProjectSummaryReportByResource]
	(
	  @ProjectNumber NVARCHAR(12) ,
	  @MilestoneId INT = NULL ,
	  @StartDate DATETIME = NULL ,
	  @EndDate DATETIME = NULL ,
	  @PersonRoleNames NVARCHAR(MAX) = NULL
	)
AS 
	BEGIN
		SET NOCOUNT ON;
		DECLARE @StartDateLocal DATETIME = NULL ,
			@EndDateLocal DATETIME = NULL ,
			@ProjectNumberLocal NVARCHAR(12) ,
			@MilestoneIdLocal INT = NULL ,
			@HolidayTimeType INT ,
			@ProjectId INT = NULL ,
			@Today DATE,
			@MilestoneStartDate DATETIME = NULL ,
			@MilestoneEndDate DATETIME = NULL,
			@FutureDate DATETIME

		SELECT @ProjectNumberLocal = @ProjectNumber,@FutureDate = dbo.GetFutureDate()
	
		SELECT  @ProjectId = P.ProjectId
		FROM    dbo.Project AS P
		WHERE   P.ProjectNumber = @ProjectNumberLocal
				AND @ProjectNumberLocal != 'P999918' --Business Development Project 

		IF ( @ProjectId IS NOT NULL ) 
			BEGIN

				SET @Today = dbo.GettingPMTime(GETUTCDATE())
				SET @MilestoneIdLocal = @MilestoneId
				SET @HolidayTimeType = dbo.GetHolidayTimeTypeId()

				IF ( @StartDate IS NOT NULL
					 AND @EndDate IS NOT NULL
				   ) 
					BEGIN
						SET @StartDateLocal = CONVERT(DATE, @StartDate)
						SET @EndDateLocal = CONVERT(DATE, @EndDate)
					END

				IF ( @MilestoneIdLocal IS NOT NULL ) 
					BEGIN
						SELECT  @MilestoneStartDate = M.StartDate ,
								@MilestoneEndDate = M.ProjectedDeliveryDate
						FROM    dbo.Milestone AS M
						WHERE   M.MilestoneId = @MilestoneIdLocal 
					END

				DECLARE @PersonRoleNamesTable TABLE
					(
					  RoleName NVARCHAR(1024)
					)
				INSERT  INTO @PersonRoleNamesTable
						SELECT  ResultString
						FROM    [dbo].[ConvertXmlStringInToStringTable](@PersonRoleNames);
				WITH PersonForeCastedHoursRoleValues
					AS
					(	
						SELECT   V.PersonId ,
								CONVERT(DECIMAL(18,2),AVG(V.BillRate)) AS BillRate, -- add
								SUM(V.PersonMilestoneDailyAmount) as PersonMilestoneDailyAmount,
								PC.Date,
								SUM(CASE WHEN PC.Date <= @Today THEN (dbo.PersonProjectedHoursPerDay(PC.DayOff,PC.CompanyDayOff,PC.TimeOffHours,V.ActualHoursPerDay)) -- MPE.HoursPerDay == ActualHoursPerDay
									ELSE 0
								END) AS ForecastedHoursUntilToday,
								SUM(dbo.PersonProjectedHoursPerDay(PC.DayOff,PC.CompanyDayOff,PC.TimeOffHours,V.ActualHoursPerDay)) AS ForecastedHours,
								MAX(ISNULL(PR.RoleValue, 0)) AS MaxRoleValue , -- add
								MIN(CAST(V.IsHourlyAmount AS INT)) MinimumValue ,
								MAX(CAST(V.IsHourlyAmount AS INT)) MaximumValue 
						FROM    v_FinancialsRetrospective V
								INNER JOIN dbo.PersonCalendarAuto PC ON PC.PersonId = V.PersonId
														AND PC.Date =V.Date
								LEFT  JOIN dbo.PersonRole AS PR ON PR.PersonRoleId = V.PersonRoleId
								
						WHERE    V.ProjectId = @ProjectId
								AND ( @MilestoneIdLocal IS NULL
										OR V.MilestoneId = @MilestoneIdLocal
									)
								AND ( ( @StartDateLocal IS NULL
										AND @EndDateLocal IS NULL
										)
										OR ( PC.Date BETWEEN @StartDateLocal AND @EndDateLocal )
									)
						GROUP BY V.PersonId,PC.Date 
					),
					TimeEntryPersons 
					AS
					(
						SELECT TE.PersonId,TE.ChargeCodeDate,
						SUM(CASE WHEN ( TEH.IsChargeable = 1 AND @ProjectNumberLocal != 'P031000'
											AND TE.ChargeCodeDate <= @Today
													) THEN TEH.ActualHours
												ELSE 0
											END) BillableHoursUntilToday,
						SUM(CASE WHEN TEH.IsChargeable = 1  AND @ProjectNumberLocal != 'P031000'
												THEN TEH.ActualHours
												ELSE 0
											END) AS BillableHours ,
						SUM(CASE WHEN TEH.IsChargeable = 0 OR @ProjectNumberLocal = 'P031000'
												THEN TEH.ActualHours
												ELSE 0
											END) AS NonBillableHours
						--TE.*, TEH.IsChargeable, TEH.ActualHours, CC.*, PTSH.PersonStatusId
						FROM TimeEntry TE
						INNER JOIN dbo.ChargeCode AS CC ON CC.Id = TE.ChargeCodeId AND CC.ProjectId = @ProjectId
						INNER JOIN dbo.TimeEntryHours AS TEH ON TEH.TimeEntryId = TE.TimeEntryId
																  AND ( ( @MilestoneIdLocal IS NULL )
																	OR ( TE.ChargeCodeDate BETWEEN @MilestoneStartDate AND @MilestoneEndDate )
																  )
																  AND ( ( @StartDateLocal IS NULL AND @EndDateLocal IS NULL )
																	OR ( TE.ChargeCodeDate BETWEEN @StartDateLocal AND @EndDateLocal )
																  )
						INNER JOIN dbo.PersonStatusHistory PTSH ON PTSH.PersonId = TE.PersonId
															  AND TE.ChargeCodeDate BETWEEN PTSH.StartDate
															  AND ISNULL(PTSH.EndDate,@FutureDate)
						INNER JOIN dbo.Person P ON P.PersonId = TE.PersonId 
													AND TE.ChargeCodeDate <= ISNULL(P.TerminationDate,@FutureDate)
													AND ( 
															CC.timeTypeId != @HolidayTimeType
															OR ( 
																	CC.timeTypeId = @HolidayTimeType
																	AND PTSH.PersonStatusId IN (1,5)
																)
														)
						WHERE ( ( @StartDateLocal IS NULL
										AND @EndDateLocal IS NULL
										)
										OR ( TE.ChargeCodeDate BETWEEN @StartDateLocal AND @EndDateLocal )
									)
						GROUP BY TE.PersonId,TE.ChargeCodeDate
					),
					ProjectResources
						AS
						(
							SELECT  PBPE.ProjectId,
									PBPE.MilestoneId,
									PBPE.StartDate,
									PBPE.EndDate,
									PBPE.HoursPerDay,
									PBPE.PersonId
							FROM ProjectBudgetPersonEntry PBPE
							WHERE PBPE.ProjectId =@ProjectId AND ( ( @StartDateLocal IS NULL
										AND @EndDateLocal IS NULL
										)
										OR ( @StartDateLocal BETWEEN PBPE.StartDate AND PBPE.EndDate)
									)
						),

						PersonBudgetHours
						AS
						(	
							SELECT   PR.PersonId,
									 PC.Date,
									 SUM(CASE WHEN P.IsStrawman = 0 THEN dbo.PersonProjectedHoursPerDay(PC.DayOff,PC.CompanyDayOff,PC.TimeOffHours,PR.HoursPerDay) ELSE 0 END)  AS ForecastedBudgetHours
							FROM	dbo.Project Pro
								LEFT JOIN ProjectResources PR ON PR.ProjectId=Pro.ProjectId
								LEFT JOIN dbo.person AS P ON P.PersonId = PR.PersonId 
								LEFT JOIN dbo.PersonCalendarAuto PC ON PC.PersonId = PR.PersonId 
										AND PC.Date BETWEEN PR.StartDate AND PR.EndDate 
						   WHERE    Pro.ProjectId = @ProjectId
								AND ( @MilestoneIdLocal IS NULL
										OR PR.MilestoneId = @MilestoneIdLocal
									)
								AND ( ( @StartDateLocal IS NULL
										AND @EndDateLocal IS NULL
										)
										OR ( PC.Date BETWEEN @StartDateLocal AND @EndDateLocal )
									)
						GROUP BY PR.PersonId, PC.Date
						),

					GroupedPersonDetails
					AS 
					(
						SELECT	ISNULL(PFR.PersonId,TEP.PersonId) AS PersonId,
								PFR.BillRate,
								SUM(PFR.PersonMilestoneDailyAmount) as PersonMilestoneDailyAmount,
								SUM(PFR.ForecastedHoursUntilToday) AS ForecastedHoursUntilToday,
								SUM(PFR.ForecastedHours) AS ForecastedHours,
								MAX(ISNULL(PFR.MaxRoleValue, 0)) AS MaxRoleValue ,
								MIN(CAST(PFR.MinimumValue AS INT)) MinimumValue ,
								MAX(CAST(PFR.MaximumValue AS INT)) MaximumValue ,
								ROUND(ISNULL(SUM(TEP.BillableHoursUntilToday),0), 2) AS BillableHoursUntilToday ,
								ROUND(ISNULL(SUM(TEP.BillableHours),0), 2) AS BillableHours ,
								ROUND(ISNULL(SUM(TEP.NonBillableHours),0), 2) AS NonBillableHours,
								ROUND(ISNULL(SUM(PBH.ForecastedBudgetHours),0), 2) AS ForecastedBudgetHours,
								SUM(CASE WHEN (PFR.Date>@Today AND TEP.BillableHours IS NULL AND TEP.NonBillableHours IS NULL) THEN PFR.ForecastedHours ELSE 0 END) AS RemainingProjectedHours
						FROM PersonForeCastedHoursRoleValues	PFR
						FULL JOIN PersonBudgetHours PBH ON PBH.PersonId = PFR.PersonId AND PBH.Date= PFR.Date
						FULL JOIN TimeEntryPersons TEP ON TEP.PersonId = PFR.PersonId AND TEP.ChargeCodeDate = PFR.Date
						GROUP BY ISNULL(PFR.PersonId,TEP.PersonId),PFR.BillRate,CASE WHEN PFR.ForecastedHours IS NOT NULL THEN 0 ELSE NULL END
					)

						
					SELECT  P.PersonId ,
							P.LastName ,
							ISNULL(P.PreferredFirstName,P.FirstName) AS FirstName,
							P.IsOffshore ,
							ISNULL(PR.Name, '') AS ProjectRoleName ,
							( 
								CASE	WHEN ( GPD.MinimumValue IS NULL ) THEN ''
										WHEN ( GPD.MinimumValue = GPD.MaximumValue AND GPD.MinimumValue = 0 ) THEN 'Fixed'
	 									WHEN ( GPD.MinimumValue = GPD.MaximumValue AND GPD.MinimumValue = 1) THEN 'Hourly'
								ELSE 'Both'
								END 
							) AS BillingType ,
							ROUND(MAX(ISNULL(GPD.ForecastedHoursUntilToday, 0)),2) AS ForecastedHoursUntilToday ,
							ROUND(MAX(ISNULL(GPD.ForecastedHours, 0)), 2) AS ForecastedHours,
							GPD.BillableHours,
							GPD.BillableHoursUntilToday,
							GPD.NonBillableHours,
							ISNULL(GPD.BillRate,0) AS BillRate,
							(CASE WHEN ( GPD.MinimumValue = GPD.MaximumValue AND GPD.MinimumValue = 0 ) THEN MAX(GPD.PersonMilestoneDailyAmount) ELSE GPD.BillableHours * ISNULL(GPD.BillRate,0) END)  as EstimatedBillings,
							P.EmployeeNumber,
							ROUND(SUM(ISNULL(GPD.ForecastedBudgetHours, 0)), 2) AS BudgetHours,
							ROUND(MAX(ISNULL(GPD.RemainingProjectedHours, 0)), 2) AS RemainingProjectedHours
					FROM  dbo.Person P
					INNER JOIN GroupedPersonDetails AS GPD ON GPD.PersonId = P.PersonId 
					--LEFT JOIN  PersonBudgetHours AS PBH ON PBH.PersonId=GPD.PersonId
					LEFT  JOIN dbo.PersonRole AS PR ON PR.RoleValue = GPD.MaxRoleValue
					WHERE	(	@PersonRoleNames IS NULL
								OR ISNULL(PR.Name, '') IN ( SELECT RoleName FROM  @PersonRoleNamesTable)
							) AND (GPD.BillableHours<>0 OR GPD.ForecastedHours <> 0 OR GPD.ForecastedBudgetHours <> 0)
					GROUP BY P.PersonId ,
							P.LastName ,
							ISNULL(P.PreferredFirstName,P.FirstName) ,
							P.IsOffshore ,
							PR.Name ,
							GPD.MinimumValue ,
							GPD.MaximumValue,
							P.EmployeeNumber,
							GPD.BillRate,
							GPD.BillableHours,
							GPD.NonBillableHours,
							GPD.BillableHoursUntilToday
					ORDER BY P.LastName
			END
		ELSE 
			BEGIN
				RAISERROR('There is no Project with this Project Number.', 16, 1)
			END
	END

