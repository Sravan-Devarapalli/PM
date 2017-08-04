CREATE PROCEDURE [dbo].[CloneProjectWithEAC]
(
	@OldProjectId	INT,
	@NewProjectId	INT	
)
AS
BEGIN
	SET NOCOUNT ON

	BEGIN TRY
	BEGIN TRAN  Tran_EACClone

	DECLARE @Today DATETIME, 
			@ProjectIdLocal INT, 
			@startDateLocal DATETIME, 
			@endDateLocal DATETIME, 
			@CurrentMonthEnd DATETIME, 
			@MilestoneCloneId INT,
			@ProjectType INT,
			@LastMonthEnd DATETIME

	SELECT @Today = CONVERT(DATE, dbo.GettingPMTime(GETUTCDATE()))
	SELECT @ProjectIdLocal =@OldProjectId
		  
	SELECT @CurrentMonthEnd =EOMONTH ( @Today )
	SELECT @LastMonthEnd=convert (date,DATEADD(MONTH, DATEDIFF(MONTH, -1, @Today)-1, -1))

	
	 SELECT @startDateLocal= p.StartDate, @endDateLocal=p.EndDate
	 FROM Project P
	 WHERE P.ProjectId=@ProjectIdLocal



	SELECT CC.ProjectId,
			TE.PersonId,
			TE.ChargeCodeDate,
			SUM(CASE WHEN TEH.IsChargeable = 1 THEN TEH.ActualHours ELSE 0 END) BillableHOursPerDay,
			SUM(CASE WHEN TEH.IsChargeable = 0 THEN TEH.ActualHours ELSE 0 END) NonBillableHoursPerDay,
			P.IsHourlyAmount
	INTO #ActualTimeEntries
	FROM TimeEntry TE
	JOIN TimeEntryHours TEH ON TEH.TimeEntryId = TE.TimeEntryId
	JOIN ChargeCode CC on CC.Id = TE.ChargeCodeId AND cc.projectid != 174
	JOIN (
			SELECT Pro.ProjectId,CAST(CASE WHEN SUM(CAST(m.IsHourlyAmount as INT)) > 0 THEN 1 ELSE 0 END AS BIT) AS IsHourlyAmount
			FROM Project Pro 
				LEFT JOIN Milestone m ON m.ProjectId = Pro.ProjectId 
			WHERE Pro.IsAllowedToShow = 1 AND Pro.ProjectId =@ProjectIdLocal
			GROUP BY Pro.ProjectId
		 ) P ON p.ProjectId = CC.ProjectId
	GROUP BY CC.ProjectId, TE.PersonId, TE.ChargeCodeDate,P.IsHourlyAmount


	SELECT  m.ProjectId,
			m.[MilestoneId],
			mp.PersonId,
			cal.Date,
			MPE.Id,
			MPE.Amount,
			m.IsHourlyAmount,	
			m.IsDefault,
			MPE.PersonRoleId,
			MPE.StartDate,
			MPE.EndDate,
			SUM(mpe.HoursPerDay) AS ActualHoursPerDay,
			SUM(CONVERT(DECIMAL(4,2),CASE WHEN cal.DayOff = 0  THEN mpe.HoursPerDay -- No Time-off and no company holiday
				WHEN (cal.companydayoff = 1) OR (cal.companydayoff = 0 AND ISNULL(cal.TimeoffHours,8) = 8) THEN 0 -- only company holiday OR person complete dayoff
				ELSE mpe.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off
			END)) AS HoursPerDay,
			SUM(CONVERT(DECIMAL(4,2),CASE WHEN cal.DayOff = 0  THEN mpe.HoursPerDay -- No Time-off and no company holiday
				WHEN (cal.companydayoff = 1) OR ( cal.companydayoff = 0 AND ISNULL(cal.TimeoffHours,8) = 8) THEN 0 -- only company holiday OR person complete dayoff
				ELSE mpe.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off
			END) * mpe.Amount) AS PersonMilestoneDailyAmount--PersonLevel
	INTO #MileStoneEntries
	FROM dbo.Project P 
	INNER JOIN dbo.[Milestone] AS m ON P.ProjectId=m.ProjectId AND p.IsAllowedToShow = 1 AND p.projectid != 174
	INNER JOIN dbo.MilestonePerson AS mp ON m.[MilestoneId] = mp.[MilestoneId]
	INNER JOIN dbo.MilestonePersonEntry AS mpe ON mp.MilestonePersonId = mpe.MilestonePersonId
	INNER JOIN dbo.PersonCalendarAuto AS cal ON cal.Date BETWEEN mpe.Startdate AND mpe.EndDate AND cal.PersonId = mp.PersonId 
	WHERE P.ProjectId =@ProjectIdLocal
	GROUP BY  m.ProjectId,m.[MilestoneId],mp.PersonId,cal.Date,m.IsHourlyAmount ,m.IsDefault,MPE.Id,MPE.Amount, MPE.PersonRoleId, MPE.StartDate, MPE.EndDate


	SELECT s.Date, s.MilestoneId, SUM(HoursPerDay) AS HoursPerDay
	INTO #CTE
	FROM #MileStoneEntries AS s
	WHERE s.IsHourlyAmount = 0
	GROUP BY s.Date, s.MilestoneId


	SELECT C.MonthStartDate, C.MonthEndDate,C.MonthNumber, s.MilestoneId, SUM(HoursPerDay) AS HoursPerMonth
	INTO #MonthlyHours
	FROM dbo.v_MilestonePersonSchedule AS s
	INNER JOIN dbo.Calendar C ON C.Date = s.Date 
	WHERE s.IsHourlyAmount = 0 and s.ProjectId=@ProjectIdLocal
	GROUP BY s.MilestoneId, C.MonthStartDate, C.MonthEndDate,C.MonthNumber


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
		WHERE FMR.MilestoneId IS NULL
		UNION ALL
	SELECT -- Milestones with a hourly amount
			mp.MilestoneId,
			mp.Date,
			mp.IsHourlyAmount,
			ISNULL(SUM(mp.Amount * mp.HoursPerDay), 0) AS MilestoneDailyAmount,
			SUM(mp.HoursPerDay) AS HoursPerDay/* Milestone Total  Hours per day*/
		FROM #MileStoneEntries mp
			INNER JOIN dbo.Project AS p ON mp.ProjectId = p.ProjectId AND mp.IsHourlyAmount = 1
	GROUP BY mp.MilestoneId, mp.Date, mp.IsHourlyAmount
)a



	SELECT	pro.ProjectId,
			Per.PersonId,
			c.Date,
			AE.BillableHOursPerDay,
			AE.NonBillableHoursPerDay,
			ISNULL(ME.IsHourlyAmount,AE.IsHourlyAmount) AS IsHourlyAmount,
			ME.ActualHoursPerDay,
			r.HoursPerDay,
			CASE
				   WHEN ME.IsHourlyAmount = 1 OR r.HoursPerDay = 0
				   THEN ME.PersonMilestoneDailyAmount
				   ELSE ISNULL(r.MilestoneDailyAmount * ME.HoursPerDay / r.HoursPerDay, r.MilestoneDailyAmount)
			   END AS PersonMilestoneDailyAmount,--Person Level Daily Amount
			 CASE
				   WHEN ME.IsHourlyAmount = 1
				   THEN ME.Amount
				   WHEN ME.IsHourlyAmount = 0 AND r.HoursPerDay = 0
				   THEN 0
				   ELSE r.MilestoneDailyAmount / r.HoursPerDay
			   END AS BillRate,
			ME.PersonRoleId,
			ME.StartDate,
			ME.EndDate,
			c.DayOff
	INTO #cteFinancialsRetrospectiveActualHours
	FROM #ActualTimeEntries AS AE --ActualEntriesByPerson
		FULL JOIN #MileStoneEntries AS ME ON ME.ProjectId = AE.ProjectId AND AE.PersonId = ME.PersonId AND ME.Date = AE.ChargeCodeDate 
		INNER JOIN dbo.Person Per ON per.PersonId = ISNULL(ME.PersonId,AE.PersonId)
		INNER JOIN dbo.Project Pro ON Pro.ProjectId = ISNULL(ME.ProjectId,AE.ProjectId) 
		INNER JOIN dbo.Calendar C ON c.Date = ISNULL(ME.Date,AE.ChargeCodeDate)
		INNER JOIN dbo.GetFutureDateTable() FD ON 1=1 --For improving query performance we are using table valued function instead of scalar function.
		LEFT JOIN #MilestoneRevenueRetrospective AS r ON ME.MilestoneId = r.MilestoneId AND c.Date = r.Date
	GROUP BY pro.ProjectId,Per.PersonId,c.Date,C.DaysInYear,AE.BillableHOursPerDay,AE.NonBillableHoursPerDay,ME.IsHourlyAmount,ME.HoursPerDay,ME.PersonMilestoneDailyAmount,
			r.HoursPerDay,r.MilestoneDailyAmount,ME.Amount,ME.Id,ME.ActualHoursPerDay,AE.IsHourlyAmount, ME.PersonRoleId, ME.StartDate, ME.EndDate, c.DayOff


	SELECT f.ProjectId,
		   f.Date, 
		   f.PersonMilestoneDailyAmount,
		   f.PersonId,
		   f.IsHourlyAmount,
		   f.BillableHOursPerDay,
  		   f.NonBillableHoursPerDay,
		   f.ActualHoursPerDay,
		   f.BillRate,
		   f.PersonRoleId,
		   f.StartDate,
		   f.EndDate,
		   f.DayOff
    INTO #FinancialsRetro
	FROM #cteFinancialsRetrospectiveActualHours f



	SELECT @ProjectType= CASE WHEN MAX(CONVERT(INT,IsHourlyAmount)) = MIN(CONVERT(INT,IsHourlyAmount)) AND MAX(CONVERT(INT,IsHourlyAmount))=1 THEN 1
		WHEN MAX(CONVERT(INT,IsHourlyAmount)) = MIN(CONVERT(INT,IsHourlyAmount)) AND MAX(CONVERT(INT,IsHourlyAmount))=0 THEN 0
		ELSE 2 END
	FROM #FinancialsRetro

		  -- Actuals 


	SELECT	f.ProjectId,
			f.PersonId,
			f.Date,
			f.PersonRoleId,
			f.StartDate,
			f.EndDate,
			SUM(CASE WHEN (f.IsHourlyAmount = 0 and f.Date<=@LastMonthEnd) THEN f.PersonMilestoneDailyAmount ELSE 0 END ) AS FixedActualRevenuePerDay,
			(ISNULL(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.BillRate* f.ActualHoursPerDay ELSE 0 END),0) / ISNULL(NULLIF(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.ActualHoursPerDay ELSE 0 END),0),1)) * MAX(CASE WHEN f.IsHourlyAmount = 1 THEN  f.BillableHOursPerDay ELSE 0 END) HourlyActualRevenuePerDay,
			SUM(ISNULL(f.BillableHOursPerDay, 0)+ ISNULL(f.NonBillableHoursPerDay,0)) AS ActualHours,
			SUM(ISNULL(CASE WHEN f.Date > @LastMonthEnd AND (f.DayOff!=1) THEN f.ActualHoursPerDay ELSE 0 END, 0)) as RemainingProjectedHours,
		    SUM(CASE WHEN (f.Date>@LastMonthEnd) THEN f.PersonMilestoneDailyAmount ELSE 0 END) AS RemainingProjectedRevenue
	INTO #ActualsAndProjectedValuesDaily
	FROM #FinancialsRetro AS f
	WHERE f.ProjectId=@ProjectIdLocal
	GROUP BY f.ProjectId, f.PersonId,f.Date, f.PersonRoleId, f.StartDate, f.EndDate
	

	SELECT	CT.ProjectId,
			CT.PersonId,
			CT.PersonRoleId,
			CT.StartDate,
			CT.EndDate,
			SUM(ISNULL(CT.HourlyActualRevenuePerDay, 0) + ISNULL(CT.FixedActualRevenuePerDay, 0)+ISNULL(CT.RemainingProjectedRevenue,0)) AS EACRevenue,
			SUM(CT.ActualHours+CT.RemainingProjectedHours) as EACHours
	INTO #EACResources
	FROM #ActualsAndProjectedValuesDaily CT
	GROUP BY CT.ProjectId, CT.PersonId, CT.PersonRoleId, CT.StartDate, 	CT.EndDate


	INSERT INTO dbo.Milestone
	    (ProjectId, Description,  StartDate, ProjectedDeliveryDate, IsHourlyAmount, MilestoneType)
	VALUES( @NewProjectId,
	    'Default Milestone (Cloned)',
	    @startDateLocal,
	    @endDateLocal,
	    CASE WHEN @ProjectType = 0 THEN 0
		ELSE 1 END,
		1)

	SET @MilestoneCloneId = SCOPE_IDENTITY()

	INSERT INTO dbo.MilestonePerson
	            (MilestoneId, PersonId)
	SELECT distinct @MilestoneCloneId, ER.PersonId
	FROM #EACResources AS ER
	
	INSERT INTO dbo.MilestonePersonEntry (MilestonePersonId, StartDate, EndDate, PersonRoleId, Amount, HoursPerDay)
	SELECT mpc.MilestonePersonId,
		   ER.StartDate,
		   ER.EndDate,
		   ER.PersonRoleId,
		   CONVERT(DECIMAL(18,2),ISNULL(ER.EACRevenue/ER.EACHours,0)),
		   CONVERT(DECIMAL (4, 2),CASE WHEN (ER.EACHours/ COUNT(cal.Date))>24 THEN 24 ELSE ER.EACHours/ COUNT(cal.Date) END)
	FROM #EACResources AS ER
	INNER JOIN dbo.MilestonePerson AS mpc
	        ON ER.PersonId = mpc.PersonId AND mpc.MilestoneId = @MilestoneCloneId	
	JOIN dbo.v_PersonCalendar AS cal ON cal.PersonId= ER.PersonId
	 WHERE cal.Date BETWEEN ER.StartDate AND ER.EndDate AND (cal.DayOff = 0 OR (cal.DayOff = 1 AND cal.CompanyDayOff = 0 AND cal.IsFloatingHoliday = 0))
	GROUP by mpc.MilestonePersonId,
		ER.StartDate,
		ER.EndDate,
		ER.PersonRoleId,
		ER.EACRevenue,
		ER.EACHours 	

	EXEC UpdateFixedMilestoneAmount @MilestoneId=@MilestoneCloneId

	DROP TABLE #ActualTimeEntries
	DROP TABLE #MileStoneEntries
	DROP TABLE #CTE
	DROP TABLE #MonthlyHours
	DROP TABLE #MilestoneRevenueRetrospective
	DROP TABLE #cteFinancialsRetrospectiveActualHours
	DROP TABLE #FinancialsRetro
	DROP TABLE #ActualsAndProjectedValuesDaily
	DROP TABLE #EACResources

	COMMIT TRAN Tran_EACClone
	END TRY
	BEGIN CATCH
		ROLLBACK TRAN Tran_EACClone
		
		DECLARE @ErrorMessage NVARCHAR(MAX)
		SET @ErrorMessage = ERROR_MESSAGE()

		RAISERROR(@ErrorMessage, 16, 1)
	END CATCH
END

