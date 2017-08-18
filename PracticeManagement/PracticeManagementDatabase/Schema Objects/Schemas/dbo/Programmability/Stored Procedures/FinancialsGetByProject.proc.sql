CREATE PROCEDURE dbo.FinancialsGetByProject
(
	@ProjectId   INT
)
AS
	SET NOCOUNT ON;

	DECLARE @ProjectIdLocal	INT

	SELECT @ProjectIdLocal = @ProjectId


	
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
			SUM(CONVERT(DECIMAL(4,2),CASE WHEN cal.DayOff = 0  THEN mpe.HoursPerDay -- No Time-off and no company holiday
				WHEN cal.companydayoff = 1 OR ISNULL(cal.TimeoffHours,8) = 8 THEN 0 -- only company holiday OR person complete dayoff
				ELSE mpe.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off
			END) * mpe.Amount) AS PersonMilestoneDailyAmount--PersonLevel
	INTO #MileStoneEntries1
		 FROM dbo.Project P
		 INNER JOIN dbo.[Milestone] AS m ON P.ProjectId=m.ProjectId AND p.IsAllowedToShow = 1 AND p.projectid != 174
		 INNER JOIN dbo.MilestonePerson AS mp ON m.[MilestoneId] = mp.[MilestoneId]
		 INNER JOIN dbo.MilestonePersonEntry AS mpe ON mp.MilestonePersonId = mpe.MilestonePersonId
		 INNER JOIN dbo.PersonCalendarAuto AS cal ON cal.Date BETWEEN mpe.Startdate AND mpe.EndDate AND cal.PersonId = mp.PersonId
	WHERE m.ProjectId = @ProjectIdLocal
	GROUP BY  m.ProjectId,m.[MilestoneId],mp.PersonId,cal.Date,m.IsHourlyAmount ,m.IsDefault,MPE.Id,MPE.Amount

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
	WHERE s.IsHourlyAmount = 0 and s.ProjectId = @ProjectIdLocal
	GROUP BY s.MilestoneId, C.MonthStartDate, C.MonthEndDate,C.MonthNumber
 
 	CREATE CLUSTERED INDEX CIX_MonthlyHours ON #MonthlyHours(MilestoneId, MonthStartDate, MonthEndDate,MonthNumber)
	

 SELECT * INTO #MilestoneRevenueRetrospective FROM(
	 SELECT 
			m.MilestoneId,
			cal.Date,
			m.IsHourlyAmount,
			ISNULL((FMR.Amount/ NULLIF(MH.HoursPerMonth,0))* d.HoursPerDay,0) AS MilestoneDailyAmount,
			p.Discount,
			d.HoursPerDay
		FROM dbo.FixedMilestoneMonthlyRevenue FMR
		JOIN Milestone M on M.MilestoneId=FMR.MilestoneId
		JOIN Project p on p.ProjectId=m.ProjectId
		INNER JOIN dbo.Calendar AS cal ON cal.Date BETWEEN FMR.StartDate AND FMR.EndDate
		JOIN #MonthlyHours MH on MH.milestoneid=M.MilestoneId AND cal.Date BETWEEN MH.MonthStartDate AND MH.MonthEndDate
		INNER JOIN #CTE AS d ON d.date = cal.Date and m.MilestoneId = d.MileStoneId
		INNER JOIN V_WorkinHoursByYear HY ON cal.date BETWEEN HY.[YearStartDate] AND HY.[YearEndDate]
		WHERE m.ProjectId = @ProjectIdLocal
		UNION ALL

	SELECT -- Milestones with a fixed amount
			m.MilestoneId,
			cal.Date,
			m.IsHourlyAmount,
			ISNULL((m.Amount/ NULLIF(MTHours.TotalHours,0))* d.HoursPerDay,0) AS MilestoneDailyAmount /* ((Milestone fixed amount/Milestone Total  Hours)* Milestone Total  Hours per day)  */,
			p.Discount,
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
		WHERE FMR.MilestoneId IS NULL AND m.ProjectId = @ProjectIdLocal
		UNION ALL
	SELECT -- Milestones with a hourly amount
			mp.MilestoneId,
			mp.Date,
			mp.IsHourlyAmount,
			ISNULL(SUM(mp.Amount * mp.HoursPerDay), 0) AS MilestoneDailyAmount,
			MAX(p.Discount) AS Discount,
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
		ME.HoursPerDay AS PersonHoursPerDay,
		ME.ActualHoursPerDay,
		r.Discount,
		r.HoursPerDay,
		CASE
	           WHEN ME.IsHourlyAmount = 1 OR r.HoursPerDay = 0
	           THEN ME.PersonMilestoneDailyAmount
	           ELSE ISNULL(r.MilestoneDailyAmount * ME.HoursPerDay / r.HoursPerDay, r.MilestoneDailyAmount)
	       END AS PersonMilestoneDailyAmount,--Person Level Daily Amount
		   CASE
	           WHEN ME.IsHourlyAmount = 1 OR r.HoursPerDay = 0
	           THEN ISNULL(ME.PersonMilestoneDailyAmount * r.Discount / 100, 0)
	           ELSE ISNULL(r.MilestoneDailyAmount * ME.HoursPerDay * r.Discount / (r.HoursPerDay * 100), r.MilestoneDailyAmount * r.Discount / 100)
	       END AS PersonDiscountDailyAmount, --Entry Level Daily Discount Amount
		   ISNULL(ME.Amount*ME.HoursPerDay, 0) as MilestonePersonAmount,
		 CASE
	           WHEN ME.IsHourlyAmount = 1
	           THEN ME.Amount
	           WHEN ME.IsHourlyAmount = 0 AND r.HoursPerDay = 0
	           THEN 0
	           ELSE r.MilestoneDailyAmount / r.HoursPerDay
		   END AS BillRate,
		     CASE 
			WHEN p.Timescale = 4
			THEN p.HourlyRate * 0.01 * ME.Amount
			ELSE p.HourlyRate
		   END AS PayRate, 	-- new payrate that takes into account that % unit is used in the Amount instead of $ unit
	      p.BonusRate,
	        SUM(CASE o.OverheadRateTypeId
	                       -- Multipliers
	                       WHEN 2 THEN
	                           (CASE
	                                 WHEN r.IsHourlyAmount = 1
	                                 THEN ME.Amount
	                                 WHEN r.IsHourlyAmount = 0 OR r.HoursPerDay = 0
	                                 THEN 0
	                                 ELSE r.MilestoneDailyAmount / r.HoursPerDay
	                             END) * o.Rate / 100 
	                       WHEN 4 THEN p.HourlyRate * o.Rate / 100
	                       -- Fixed
	                       WHEN 3 THEN (o.Rate * 12 / (C.DaysInYear * 8)) 
	                       ELSE o.Rate
	                   END) AS OverheadRate,
	           	ISNULL(p.HourlyRate * MLFO.Rate / 100,0) MLFOverheadRate,
			(CASE WHEN p.Timescale = 2
				 THEN ISNULL(p.HourlyRate * p.VacationDays * ME.HoursPerDay,0)/(C.DaysInYear * 8)
			ELSE 0 END)  VacationRate
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
			p.Timescale,p.HourlyRate,p.BonusRate,p.VacationDays,
			r.HoursPerDay,r.MilestoneDailyAmount,r.Discount,MLFO.Rate,ME.ActualHoursPerDay, ME.Amount
	
	CREATE CLUSTERED INDEX CIX_cteFinancialsRetrospectiveActualHours ON #cteFinancialsRetrospective(ProjectId,
		PersonId,
		Date,
		IsHourlyAmount,
		PersonHoursPerDay,
		ActualHoursPerDay,
		Discount,
		HoursPerDay)
	

	SELECT f.ProjectId,
		   f.MilestoneId,
		   f.Date, 
		   f.PersonMilestoneDailyAmount,
		   f.MilestonePersonAmount,
		   f.PersonDiscountDailyAmount,
		   (ISNULL(f.PayRate, 0) + ISNULL(f.OverheadRate, 0)+ISNULL(f.BonusRate,0)+ISNULL(f.VacationRate,0)) SLHR,
		   ISNULL(f.PayRate,0) PayRate,
		   f.MLFOverheadRate,
		   f.PersonHoursPerDay,
		   f.PersonId,
		   f.Discount
	INTO #FinancialsRetro
	FROM #cteFinancialsRetrospective f
	WHERE f.ProjectId=@ProjectIdLocal

	;WITH ProjectFinancials
	AS
	(
	SELECT f.ProjectId,
	       dbo.MakeDate(YEAR(MIN(f.Date)), MONTH(MIN(f.Date)), 1) AS FinancialDate,
	       dbo.MakeDate(YEAR(MIN(f.Date)), MONTH(MIN(f.Date)), dbo.GetDaysInMonth(MIN(f.Date))) AS MonthEnd,

	       SUM(f.PersonMilestoneDailyAmount) AS Revenue,

	       SUM(f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount) AS RevenueNet,
	       
		   SUM(f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount -
						(CASE WHEN f.SLHR  >= f.PayRate + f.MLFOverheadRate 
							  THEN f.SLHR ELSE f.PayRate + f.MLFOverheadRate END) 
					    *ISNULL(f.PersonHoursPerDay, 0)) GrossMargin,
		   
		   ISNULL(SUM((CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE f.PayRate + f.MLFOverheadRate END)*ISNULL(f.PersonHoursPerDay, 0)),0) Cogs,

	       ISNULL(SUM(f.PersonHoursPerDay), 0) AS Hours
 	  FROM #FinancialsRetro AS f
	 WHERE f.ProjectId = @ProjectIdLocal
	GROUP BY f.ProjectId
	)
	SELECT
		P.ProjectId,
		P.StartDate FinancialDate,
		p.EndDate MonthEnd,
		Convert(DECIMAL(18,2),ISNULL(pf.Revenue,0)) as 'Revenue',
		Convert(DECIMAL(18,2),ISNULL(pf.RevenueNet,0)+ISNULL(PE.ReimbursedExpenseSum,0)) as 'RevenueNet',
		CASE WHEN (pr.IsCompanyInternal = 1) THEN 0
		ELSE Convert(DECIMAL(18,2),ISNULL(pf.Cogs,0)) END AS 'Cogs',
		Convert(DECIMAL(18,2),ISNULL(pf.GrossMargin,0)+((ISNULL(PE.ReimbursedExpenseSum,0)-ISNULL(PE.ExpenseSum,0))))  as 'GrossMargin',
		ISNULL(pf.Hours,0) Hours,
		Convert(DECIMAL(18,2),ISNULL(PE.ExpenseSum,0)) Expense,
		Convert(DECIMAL(18,2),ISNULL(PE.ReimbursedExpenseSum,0)) ReimbursedExpense
	FROM  Project p 
	JOIN Practice pr ON (pr.PracticeId = p.PracticeId)
	LEFT JOIN ProjectFinancials pf
	ON (p.ProjectId = pf.ProjectId)
	LEFT JOIN v_ProjectTotalExpenses PE ON P.ProjectId = PE.ProjectId
	WHERE (pf.ProjectId IS NOT NULL OR PE.ProjectId IS NOT NULL) AND P.ProjectId =@ProjectIdLocal
			AND P.StartDate IS NOT  NULL AND P.EndDate IS NOT NULL	
	
	DROP TABLE #MileStoneEntries1
	DROP TABLE #CTE
	DROP TABLE #MonthlyHours
	DROP TABLE #MilestoneRevenueRetrospective
	DROP TABLE #cteFinancialsRetrospective
	DROP TABLE #FinancialsRetro

