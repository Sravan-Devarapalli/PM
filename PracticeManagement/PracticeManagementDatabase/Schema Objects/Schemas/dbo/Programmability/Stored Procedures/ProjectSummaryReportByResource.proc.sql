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


					SELECT  m.ProjectId,
							m.[MilestoneId],
							mp.PersonId As PersonId,
							cal.Date,
							MPE.Id,
							MPE.Amount,
							m.IsHourlyAmount,	
							m.IsDefault,
							SUM(mpe.HoursPerDay) AS ActualHoursPerDay,
							SUM(CONVERT(DECIMAL(4,2),CASE WHEN cal.DayOff = 0  THEN mpe.HoursPerDay -- No Time-off and no company holiday
								WHEN cal.companydayoff = 1 OR ISNULL(cal.TimeoffHours,8) = 8 THEN 0 -- only company holiday OR person complete dayoff
								ELSE mpe.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off
							END)) AS HoursPerDay,
							SUM(CONVERT(DECIMAL(5,3),CASE WHEN cal.DayOff = 0  THEN mpe.HoursPerDay -- No Time-off and no company holiday
								WHEN cal.companydayoff = 1 OR ISNULL(cal.TimeoffHours,8) = 8 THEN 0 -- only company holiday OR person complete dayoff
								ELSE mpe.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off
							END) * mpe.Amount) AS PersonMilestoneDailyAmount,--PersonLevel
							mpe.PersonRoleId
						
					INTO #MileStoneEntries1
						 FROM dbo.Project P
						 INNER JOIN dbo.[Milestone] AS m ON P.ProjectId=m.ProjectId AND p.IsAllowedToShow = 1 AND p.projectid != 174
						 INNER JOIN dbo.MilestonePerson AS mp ON m.[MilestoneId] = mp.[MilestoneId]
						 INNER JOIN dbo.MilestonePersonEntry AS mpe ON mp.MilestonePersonId = mpe.MilestonePersonId
						 INNER JOIN dbo.PersonCalendarAuto AS cal ON cal.Date BETWEEN mpe.Startdate AND mpe.EndDate AND cal.PersonId = mp.PersonId
					WHERE p.ProjectId = @ProjectId
					GROUP BY  m.ProjectId,m.[MilestoneId],mp.PersonId,cal.Date,m.IsHourlyAmount ,m.IsDefault,MPE.Id,MPE.Amount, mpe.PersonRoleId

					CREATE CLUSTERED INDEX cix_MileStoneEntries1 ON #MileStoneEntries1( ProjectId,[MilestoneId],PersonId,[Date],IsHourlyAmount ,IsDefault,Id,Amount)


					SELECT s.Date, s.MilestoneId, SUM(HoursPerDay) AS HoursPerDay
					INTO #CTE
					FROM #MileStoneEntries1 AS s
					WHERE s.IsHourlyAmount = 0
					GROUP BY s.Date, s.MilestoneId

					CREATE CLUSTERED INDEX CIX_CTE ON #CTE(Date,MilestoneId)
	
	
					SELECT C.MonthStartDate, C.MonthEndDate,C.MonthNumber, s.MilestoneId, SUM(HoursPerDay) AS HoursPerMonth
					INTO #MonthlyHours
					FROM dbo.v_MilestonePersonSchedule AS s
					INNER JOIN dbo.Calendar C ON C.Date = s.Date 
					WHERE s.IsHourlyAmount = 0 and s.ProjectId = @ProjectId
					GROUP BY s.MilestoneId, C.MonthStartDate, C.MonthEndDate,C.MonthNumber
 
 					CREATE CLUSTERED INDEX CIX_MonthlyHours ON #MonthlyHours(MilestoneId, MonthStartDate, MonthEndDate,MonthNumber)
	

				 SELECT * INTO #MilestoneRevenueRetrospective FROM(
					 SELECT 
							m.MilestoneId,
							cal.Date,
							m.IsHourlyAmount,
							ISNULL((FMR.Amount/ NULLIF(MH.HoursPerMonth,0))* d.HoursPerDay,0) AS MilestoneDailyAmount,
							d.HoursPerDay
						FROM dbo.FixedMilestoneMonthlyRevenue FMR
						JOIN Milestone M on M.MilestoneId=FMR.MilestoneId
						JOIN Project p on p.ProjectId=m.ProjectId
						INNER JOIN dbo.Calendar AS cal ON cal.Date BETWEEN FMR.StartDate AND FMR.EndDate
						JOIN #MonthlyHours MH on MH.milestoneid=M.MilestoneId AND cal.Date BETWEEN MH.MonthStartDate AND MH.MonthEndDate
						INNER JOIN #CTE AS d ON d.date = cal.Date and m.MilestoneId = d.MileStoneId
						INNER JOIN V_WorkinHoursByYear HY ON cal.date BETWEEN HY.[YearStartDate] AND HY.[YearEndDate]
						WHERE m.ProjectId = @ProjectId
						UNION ALL

					SELECT -- Milestones with a fixed amount
							m.MilestoneId,
							cal.Date,
							m.IsHourlyAmount,
							ISNULL((m.Amount/ NULLIF(MTHours.TotalHours,0))* d.HoursPerDay,0) AS MilestoneDailyAmount /* ((Milestone fixed amount/Milestone Total  Hours)* Milestone Total  Hours per day)  */,
							d.HoursPerDay/* Milestone Total  Hours per day*/
						FROM dbo.Project AS p 
							INNER JOIN dbo.Milestone AS m ON m.ProjectId = p.ProjectId AND P.IsAdministrative = 0 AND P.ProjectId != 174 AND  m.IsHourlyAmount = 0
							INNER JOIN dbo.Calendar AS cal ON cal.Date BETWEEN m.StartDate AND m.ProjectedDeliveryDate
							INNER JOIN (
											SELECT s.MilestoneId, SUM(s.HoursPerDay) AS TotalHours
											FROM #CTE AS s 
											GROUP BY s.MilestoneId
										) AS MTHours  ON MTHours.MilestoneId  = m.MilestoneId
							INNER JOIN #CTE AS d ON d.date = cal.Date and m.MilestoneId = d.MileStoneId
							LEFT JOIN (select distinct milestoneid from dbo.FixedMilestoneMonthlyRevenue) FMR on m.MilestoneId=FMR.MilestoneId
						WHERE FMR.MilestoneId IS NULL AND m.ProjectId = @ProjectId
						UNION ALL
					SELECT -- Milestones with a hourly amount
							mp.MilestoneId,
							mp.Date,
							mp.IsHourlyAmount,
							ISNULL(SUM(mp.Amount * mp.HoursPerDay), 0) AS MilestoneDailyAmount,
							SUM(mp.HoursPerDay) AS HoursPerDay/* Milestone Total  Hours per day*/
						FROM #MileStoneEntries1 mp
							INNER JOIN dbo.Project AS p ON mp.ProjectId = p.ProjectId AND mp.IsHourlyAmount = 1
					GROUP BY mp.MilestoneId, mp.Date, mp.IsHourlyAmount
					)a


						CREATE CLUSTERED INDEX CIX_MilestoneRevenueRetrospective ON #MilestoneRevenueRetrospective(MilestoneId, Date, IsHourlyAmount)
	
				SELECT	pro.ProjectId,
						ME.MilestoneId,
						Per.PersonId As PersonId,
						c.Date,
						ME.IsHourlyAmount AS IsHourlyAmount,
						ME.ActualHoursPerDay,
						r.HoursPerDay,
						CASE WHEN ME.IsHourlyAmount = 1 OR r.HoursPerDay = 0
							 THEN ME.PersonMilestoneDailyAmount
							 ELSE ISNULL(r.MilestoneDailyAmount * ME.HoursPerDay / r.HoursPerDay, r.MilestoneDailyAmount)
						END AS PersonMilestoneDailyAmount,--Person Level Daily Amount
						 
						CASE WHEN ME.IsHourlyAmount = 1
						     THEN ME.Amount
							 WHEN ME.IsHourlyAmount = 0 AND r.HoursPerDay = 0
							 THEN 0
							 ELSE r.MilestoneDailyAmount / r.HoursPerDay
						END AS BillRate,
						ME.PersonRoleId 
				INTO #cteFinancialsRetrospective
				FROM  #MileStoneEntries1 AS ME 
						INNER JOIN dbo.Person Per ON per.PersonId = ME.PersonId
						INNER JOIN dbo.Project Pro ON Pro.ProjectId = ME.ProjectId
						INNER JOIN dbo.Calendar C ON c.Date = ME.Date
						INNER JOIN dbo.GetFutureDateTable() FD ON 1=1 --For improving query performance we are using table valued function instead of scalar function.
						LEFT JOIN dbo.[v_PersonPayRetrospective] AS p ON p.PersonId = per.PersonId AND p.Date = c.Date
						LEFT JOIN v_MLFOverheadFixedRateTimescale MLFO ON MLFO.TimescaleId = p.Timescale AND c.Date BETWEEN MLFO.StartDate AND ISNULL(MLFO.EndDate,FD.FutureDate)
						LEFT JOIN dbo.v_OverheadFixedRateTimescale AS o ON p.Date BETWEEN o.StartDate AND ISNULL(o.EndDate, FD.FutureDate) AND o.TimescaleId = p.Timescale
						LEFT JOIN #MilestoneRevenueRetrospective AS r ON ME.MilestoneId = r.MilestoneId AND c.Date = r.Date
					GROUP BY pro.ProjectId,ME.MilestoneId, Per.PersonId,c.Date,C.DaysInYear,ME.IsHourlyAmount,ME.HoursPerDay,ME.PersonMilestoneDailyAmount,
							p.Timescale,p.HourlyRate,p.VacationDays,
							r.HoursPerDay,r.MilestoneDailyAmount, MLFO.Rate,ME.ActualHoursPerDay, ME.Amount, ME.PersonRoleId
	
					CREATE CLUSTERED INDEX CIX_cteFinancialsRetrospectiveActualHours ON #cteFinancialsRetrospective(ProjectId,
						PersonId,
						Date,
						IsHourlyAmount,
						ActualHoursPerDay,
						HoursPerDay,
						PersonRoleId)
	

					SELECT f.ProjectId,
						   f.MilestoneId,
						   f.Date, 
						   f.PersonMilestoneDailyAmount,
						   f.PersonId,
						   f.PersonRoleId,
						   f.ActualHoursPerDay,
						   f.BillRate,
						   f.IsHourlyAmount
					INTO #FinancialsRetro
					FROM #cteFinancialsRetrospective f
					WHERE f.ProjectId=@ProjectId


				;WITH PersonForeCastedHoursRoleValues
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
						FROM    #FinancialsRetro V
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
										OR (  PBPE.StartDate between  @StartDateLocal AND @EndDateLocal)
									)
						),

						PersonBudgetHours
						AS
						(	
							SELECT   PR.PersonId,
									 PC.Date,
									 SUM(dbo.PersonProjectedHoursPerDay(PC.DayOff,PC.CompanyDayOff,PC.TimeOffHours,PR.HoursPerDay))  AS ForecastedBudgetHours
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
					INTO #GroupedResult
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


					SELECT  T.PersonId ,
							T.LastName ,
							T.FirstName,
							T.IsOffshore ,
							T.ProjectRoleName ,
							T.BillingType ,
							SUM(T.ForecastedHoursUntilToday) as ForecastedHoursUntilToday,
							SUM(T.ForecastedHours) as ForecastedHours,
							SUM(T.BillableHours) as BillableHours,
							SUM(T.BillableHoursUntilToday) as BillableHoursUntilToday,
							SUM(T.NonBillableHours) as NonBillableHours,
							AVG(T.BillRate) as  BillRate,
							CONVERT(DECIMAL(18,5), SUM(T.EstimatedBillings)) as EstimatedBillings,
							T.EmployeeNumber,
							SUM(T.BudgetHours) as BudgetHours,
							SUM(T.RemainingProjectedHours) as RemainingProjectedHours
					FROM #GroupedResult T
					WHERE T.BillRate= 0
					GROUP BY T.PersonId ,
							T.LastName ,
							T.FirstName,
							T.IsOffshore ,
							T.ProjectRoleName ,
							T.BillingType ,
							T.EmployeeNumber
					UNION ALL
					SELECT  T.PersonId ,
							T.LastName ,
							T.FirstName,
							T.IsOffshore ,
							T.ProjectRoleName ,
							T.BillingType ,
							SUM(T.ForecastedHoursUntilToday) as ForecastedHoursUntilToday,
							SUM(T.ForecastedHours) as ForecastedHours,
							SUM(T.BillableHours) as BillableHours,
							SUM(T.BillableHoursUntilToday) as BillableHoursUntilToday,
							SUM(T.NonBillableHours) as NonBillableHours,
							CASE WHEN T.BillingType!='Hourly' AND SUM(T.BillableHours+T.NonBillableHours) != 0 THEN SUM(T.EstimatedBillings)/(SUM(T.BillableHours+T.NonBillableHours))
								 WHEN T.BillingType='Hourly' THEN T.BillRate
								 ELSE 0 END as  BillRate,
							CONVERT(DECIMAL(18,5), SUM(T.EstimatedBillings)) as EstimatedBillings,
							T.EmployeeNumber,
							SUM(T.BudgetHours) as BudgetHours,
							SUM(T.RemainingProjectedHours) as RemainingProjectedHours
					FROM #GroupedResult T
					WHERE T.BillRate <> 0
					GROUP BY T.PersonId ,
							T.LastName ,
							T.FirstName,
							T.IsOffshore ,
							T.ProjectRoleName ,
							T.BillingType ,
							T.EmployeeNumber,
							T.BillRate

					DROP TABLE #MileStoneEntries1
					DROP TABLE #CTE
					DROP TABLE #MonthlyHours
					DROP TABLE #MilestoneRevenueRetrospective
					DROP TABLE #cteFinancialsRetrospective
					DROP TABLE #FinancialsRetro
					DROP TABLE #GroupedResult
			END
		ELSE 
			BEGIN
				RAISERROR('There is no Project with this Project Number.', 16, 1)
			END
	END

