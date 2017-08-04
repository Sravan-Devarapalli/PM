CREATE PROCEDURE [dbo].[FinancialSummaryGetByProject]
(
	@ProjectId   INT
)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ProjectIdLocal	INT,
			@Today	DATETIME,
			@CurrentMonthEnd DATETIME,
			@LastMonthEnd  DATETIME

	SELECT @ProjectIdLocal = @ProjectId
	SELECT @Today=dbo.GettingPMTime(GETUTCDATE())
	SELECT @CurrentMonthEnd =EOMONTH ( @Today )
	SELECT @LastMonthEnd=convert (date,DATEADD(MONTH, DATEDIFF(MONTH, -1, @Today)-1, -1))
	
	SELECT CC.ProjectId,
			TE.PersonId As PersonId,
			TE.ChargeCodeDate,
			SUM(CASE WHEN TEH.IsChargeable = 1 THEN TEH.ActualHours ELSE 0 END) BillableHOursPerDay,
			SUM(CASE WHEN TEH.IsChargeable = 0 THEN TEH.ActualHours ELSE 0 END) NonBillableHoursPerDay,
			P.IsHourlyAmount
			INTO #ActualTimeEntries
	FROM TimeEntry TE
	JOIN TimeEntryHours TEH ON TEH.TimeEntryId = TE.TimeEntryId
	JOIN ChargeCode CC on CC.Id = TE.ChargeCodeId AND cc.projectid <> 174
	JOIN (
			SELECT Pro.ProjectId,CAST(CASE WHEN SUM(CAST(m.IsHourlyAmount as INT)) > 0 THEN 1 ELSE 0 END AS BIT) AS IsHourlyAmount
			FROM Project Pro
				LEFT JOIN Milestone m ON m.ProjectId = Pro.ProjectId			
			WHERE Pro.ProjectId=  @ProjectIdLocal 
			GROUP BY Pro.ProjectId
		 ) P ON p.ProjectId = CC.ProjectId
	WHERE TE.ChargeCodeDate<=@LastMonthEnd
	GROUP BY CC.ProjectId, TE.PersonId, TE.ChargeCodeDate,P.IsHourlyAmount

	CREATE CLUSTERED INDEX CIX_ActualTimeEntries ON #ActualTimeEntries(ProjectId, PersonId, ChargeCodeDate,IsHourlyAmount)


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
		WHERE P.ProjectId=  @ProjectIdLocal 
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
	WHERE s.IsHourlyAmount = 0
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
		WHERE FMR.MilestoneId IS NULL
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
		Per.PersonId As PersonId,
		c.Date,
		AE.BillableHOursPerDay,
		AE.NonBillableHoursPerDay,
		ISNULL(ME.IsHourlyAmount,AE.IsHourlyAmount) AS IsHourlyAmount,
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
			INTO #cteFinancialsRetrospectiveActualHours
FROM #ActualTimeEntries AS AE --ActualEntriesByPerson
		FULL JOIN #MileStoneEntries1 AS ME ON ME.ProjectId = AE.ProjectId AND AE.PersonId = ME.PersonId AND ME.Date = AE.ChargeCodeDate 
		INNER JOIN dbo.Person Per ON per.PersonId = ISNULL(ME.PersonId,AE.PersonId)
		INNER JOIN dbo.Project Pro ON Pro.ProjectId = ISNULL(ME.ProjectId,AE.ProjectId) 
		INNER JOIN dbo.Calendar C ON c.Date = ISNULL(ME.Date,AE.ChargeCodeDate)
		INNER JOIN dbo.GetFutureDateTable() FD ON 1=1 --For improving query performance we are using table valued function instead of scalar function.
		LEFT JOIN dbo.[v_PersonPayRetrospective] AS p ON p.PersonId = per.PersonId AND p.Date = c.Date
		LEFT JOIN v_MLFOverheadFixedRateTimescale MLFO ON MLFO.TimescaleId = p.Timescale AND c.Date BETWEEN MLFO.StartDate AND ISNULL(MLFO.EndDate,FD.FutureDate)
		LEFT JOIN dbo.v_OverheadFixedRateTimescale AS o ON p.Date BETWEEN o.StartDate AND ISNULL(o.EndDate, FD.FutureDate) AND o.TimescaleId = p.Timescale
		LEFT JOIN #MilestoneRevenueRetrospective AS r ON ME.MilestoneId = r.MilestoneId AND c.Date = r.Date
	GROUP BY pro.ProjectId,Per.PersonId,c.Date,C.DaysInYear,AE.BillableHOursPerDay,AE.NonBillableHoursPerDay,ME.IsHourlyAmount,ME.HoursPerDay,ME.PersonMilestoneDailyAmount,
			p.Timescale,p.HourlyRate,p.BonusRate,p.VacationDays,
			r.HoursPerDay,r.MilestoneDailyAmount,r.Discount,MLFO.Rate,ME.Amount,ME.Id,ME.ActualHoursPerDay,AE.IsHourlyAmount
	
	CREATE CLUSTERED INDEX CIX_cteFinancialsRetrospectiveActualHours ON #cteFinancialsRetrospectiveActualHours(ProjectId,
		PersonId,
		Date,
		BillableHOursPerDay,
		NonBillableHoursPerDay,
		IsHourlyAmount,
		PersonHoursPerDay,
		ActualHoursPerDay,
		Discount,
		HoursPerDay)
	

	SELECT f.ProjectId,
		   f.Date, 
		   f.PersonMilestoneDailyAmount,
		   f.PersonDiscountDailyAmount,
		   (ISNULL(f.PayRate, 0) + ISNULL(f.OverheadRate, 0)+ISNULL(f.BonusRate,0)+ISNULL(f.VacationRate,0)) SLHR,
		   ISNULL(f.PayRate,0) PayRate,
		   f.MLFOverheadRate,
		   f.PersonHoursPerDay,
		   f.PersonId As PersonId,
		   f.Discount,
		   f.IsHourlyAmount,
		   f.BillableHOursPerDay,
  		   f.NonBillableHoursPerDay,
		   f.ActualHoursPerDay,
		   f.BillRate
		   INTO #FinancialsRetro
	FROM #cteFinancialsRetrospectiveActualHours f
	WHERE f.ProjectId=@ProjectIdLocal

	SELECT @ProjectIdLocal as ProjectId,
	       SUM(f.PersonMilestoneDailyAmount) AS Revenue,
	       SUM(f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount) AS RevenueNet,
		   SUM(f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount -
						(CASE WHEN f.SLHR  >= f.PayRate + f.MLFOverheadRate 
							  THEN f.SLHR ELSE f.PayRate + f.MLFOverheadRate END)*ISNULL(f.PersonHoursPerDay, 0)) GrossMargin,
		   ISNULL(SUM((CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE f.PayRate + f.MLFOverheadRate END)*ISNULL(f.PersonHoursPerDay, 0)),0) Cogs,
	       ISNULL(SUM(f.PersonHoursPerDay), 0) AS Hours
	INTO #RemainingProjectedFinancials
 	FROM #FinancialsRetro AS f
	WHERE f.Date >  @LastMonthEnd
						

	CREATE CLUSTERED INDEX CIX_RemainingProjectedFinancials ON #RemainingProjectedFinancials(ProjectId)

	SELECT @ProjectIdLocal AS ProjectId,
		   SUM(CASE WHEN PDE.date>@LastMonthEnd THEN PDE.EstimatedAmount ELSE 0 END) as EstimatedAmount,
		   SUM( CASE WHEN PDE.date>@LastMonthEnd THEN  PDE.EstimatedReimbursement ELSE 0 END ) as EstimatedReimbursement,
		   SUM(CASE WHEN PDE.date<=@LastMonthEnd THEN PDE.ActualExpense ELSE 0 END ) as ActualExpense,
		   SUM(CASE WHEN PDE.date<=@LastMonthEnd THEN PDE.ActualReimbursement ELSE 0 END ) as ActualReimbursement
	INTO #ProjectExpenses
	FROM v_ProjectDailyExpenses PDE
	WHERE PDE.ProjectId = @ProjectIdLocal

	CREATE CLUSTERED INDEX CIX_ProjectExpenses ON #ProjectExpenses(ProjectId)
		
	SELECT	@ProjectIdLocal AS ProjectId,
			f.personid,
			f.Date,
		   SUM(f.PersonMilestoneDailyAmount) AS Revenue,
	       SUM(f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount) AS RevenueNet,
		   SUM(f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount -
						(CASE WHEN f.SLHR  >= f.PayRate + f.MLFOverheadRate 
							  THEN f.SLHR ELSE f.PayRate + f.MLFOverheadRate END)*ISNULL(f.PersonHoursPerDay, 0)) GrossMargin,
		   ISNULL(SUM((CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE f.PayRate + f.MLFOverheadRate END)*ISNULL(f.PersonHoursPerDay, 0)),0) Cogs,
		   SUM(CASE WHEN (f.IsHourlyAmount = 0 and f.Date<=@LastMonthEnd) THEN f.PersonMilestoneDailyAmount ELSE 0 END ) AS FixedActualRevenuePerDay,
		   ISNULL(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.BillRate* f.BillableHOursPerDay ELSE 0 END),0) AS HourlyActualRevenuePerDay,
		
		   ISNULL(SUM(CASE WHEN (f.IsHourlyAmount = 0 and f.Date<=@LastMonthEnd) THEN f.PersonMilestoneDailyAmount ELSE 0 END),0)- (
						ISNULL( MAX( CASE WHEN f.IsHourlyAmount = 0 THEN  f.BillableHOursPerDay + f.NonBillableHoursPerDay ELSE 0 END ),0) * 
						ISNULL( CASE WHEN ISNULL(SUM(CASE WHEN f.IsHourlyAmount = 0 THEN f.ActualHoursPerDay ELSE 0 END),0) > 0 
									THEN SUM((CASE WHEN f.SLHR >=  f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END)* CASE WHEN f.IsHourlyAmount = 0 THEN f.ActualHoursPerDay ELSE 0 END) / 
									SUM(CASE WHEN f.IsHourlyAmount = 0 THEN f.ActualHoursPerDay ELSE 0 END)
									ELSE SUM((CASE WHEN f.SLHR >=  f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END)) / ISNULL(COUNT(f.PayRate),1)  
								END ,0)
					) AS FixedActualMarginPerDay,
		
	       ISNULL(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.BillRate* f.BillableHOursPerDay ELSE 0 END),0)
				 -  (
						MAX( CASE WHEN f.IsHourlyAmount = 1 THEN  f.BillableHOursPerDay + f.NonBillableHoursPerDay ELSE 0 END ) * 
						ISNULL( CASE WHEN ISNULL(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.ActualHoursPerDay ELSE 0 END),0) > 0 
									THEN SUM((CASE WHEN f.SLHR >=  f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END)* CASE WHEN f.IsHourlyAmount = 1 THEN f.ActualHoursPerDay ELSE 0 END) / 
									SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.ActualHoursPerDay ELSE 0 END)
									ELSE SUM((CASE WHEN f.SLHR >=  f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END)) / ISNULL(COUNT(f.PayRate),1)  
								END ,0)
					) AS HourlyActualMarginPerDay,
		  	min(f.Discount) as Discount
	INTO #ActualAndProjectedValuesDaily
	FROM #FinancialsRetro AS f
	GROUP BY f.personid, f.Date

	CREATE CLUSTERED INDEX CIX_ActualAndProjectedValuesDaily ON #ActualAndProjectedValuesDaily(ProjectId, Date)


	SELECT  @ProjectIdLocal AS ProjectId, 
			SUM(ISNULL(CT.Revenue, 0)) as Revenue,
			SUM(ISNULL(CT.RevenueNet, 0)) as RevenueNet,
			SUM(ISNULL(CT.GrossMargin, 0)) as GrossMargin,
			SUM(ISNULL(CT.Cogs, 0)) as Cogs,
			SUM(ISNULL(CT.HourlyActualRevenuePerDay, 0) + ISNULL(CT.FixedActualRevenuePerDay, 0)) AS ActualRevenue,
			SUM(ISNULL(CT.HourlyActualMarginPerDay, 0) + ISNULL(CT.FixedActualMarginPerDay, 0)) as ActualMargin,
			MAX(ISNULL(CT.Discount, 0)) Discount
	INTO #ActualAndProjectedValuesMonthly
	FROM #ActualAndProjectedValuesDaily CT
	INNER JOIN dbo.Calendar C ON C.Date = CT.Date	

	--Budget
	Select 1 AS FinanceType,
		   PBH.ProjectId,
		   ISNULL(PBH.Revenue,0)  as 'Revenue',
		   ISNULL(PBH.Revenue,0)-ISNULL((ISNULL(PBH.Revenue,0)*ISNULL(PBH.Discount,0))/100,0) AS 'RevenueNet',
		   ISNULL(PBH.COGS,0)  AS 'Cogs',
		   (ISNULL(PBH.Revenue,0)+ISNULL(PBH.ReimbursedExpense,0)-ISNULL(PBH.Expense,0)-ISNULL((ISNULL(PBH.Revenue,0)*ISNULL(PBH.Discount,0))/100,0)-PBH.COGS) as 'GrossMargin',
		   PBH.Expense,
		   PBH.ReimbursedExpense
	FROM ProjectBudgetHistory PBH
	WHERE PBH.ProjectId=@ProjectIdLocal AND PBH.IsActive=1 AND PBH.MilestoneId IS NULL

	UNION ALL
	--Projected Revenues
	SELECT 2 AS FinanceType,
		Pf.ProjectId,
		CONVERT(DECIMAL(18,2), ISNULL(pf.Revenue,0) ) as 'Revenue',
		CONVERT(DECIMAL(18,2), ISNULL(pf.RevenueNet,0)+ISNULL(PE.ReimbursedExpenseSum,0)) as 'RevenueNet',
		CONVERT(DECIMAL(18,2), ISNULL(pf.Cogs,0))  AS 'Cogs',
		CONVERT(DECIMAL(18,2), ISNULL(pf.GrossMargin,0)+((ISNULL(PE.ReimbursedExpenseSum,0)-ISNULL(PE.ExpenseSum,0))))  as 'GrossMargin',
		ISNULL(PE.ExpenseSum,0) Expense,
		ISNULL(PE.ReimbursedExpenseSum,0) ReimbursedExpense
	FROM   #ActualAndProjectedValuesMonthly pf 
	LEFT JOIN v_ProjectTotalExpenses PE ON Pf.ProjectId = PE.ProjectId			

	UNION ALL

	--Remaining Projected Financials
	SELECT 3 AS FinanceType,
		rpf.ProjectId,
		CONVERT(DECIMAL(18,2), ISNULL(rpf.Revenue,0))  as 'Revenue',
		CONVERT(DECIMAL(18,2), ISNULL(rpf.RevenueNet,0)+ISNULL(PE.EstimatedReimbursement,0)) as 'RevenueNet',
		CONVERT(DECIMAL(18,2),ISNULL(rpf.Cogs,0)) AS 'Cogs',
		CONVERT(DECIMAL(18,2),ISNULL(rpf.GrossMargin,0)+((ISNULL(PE.EstimatedReimbursement,0)-ISNULL(PE.EstimatedAmount,0)))) as 'GrossMargin',
		ISNULL(PE.EstimatedAmount,0) Expense,
		ISNULL(PE.EstimatedReimbursement,0) ReimbursedExpense
	FROM   #RemainingProjectedFinancials rpf 
	LEFT JOIN #ProjectExpenses PE ON rpf.ProjectId = PE.ProjectId

	UNION ALL
	--Actual
		SELECT 	4 AS FinanceType,
			APV.ProjectId,
			CONVERT(DECIMAL(18,2), ISNULL(APV.ActualRevenue,0)) Revenue,
			CONVERT(DECIMAL(18,2),(ISNULL(APV.ActualRevenue,0) +ISNULL(AE.ActualReimbursement,0)-(ISNULL(APV.ActualRevenue,0) * ISNULL(APV.Discount,0)/100))) AS 'RevenueNet',
			CONVERT(DECIMAL(18,2),(ISNULL(APV.ActualRevenue,0) -ISNULL(APV.ActualMargin,0))) AS 'Cogs',
			CONVERT(DECIMAL(18,2), ISNULL(APV.ActualMargin,0) - (ISNULL(APV.ActualRevenue,0) * ISNULL(APV.Discount,0)/100) + ((ISNULL(AE.ActualReimbursement,0)-ISNULL(AE.ActualExpense,0)))) 'GrossMargin',
			ISNULL(AE.ActualExpense,0),
			ISNULL(AE.ActualReimbursement,0)
	FROM #ActualAndProjectedValuesMonthly APV
	LEFT JOIN #ProjectExpenses AE ON APV.ProjectId =AE.ProjectId

	DROP TABLE #FinancialsRetro
	DROP TABLE #RemainingProjectedFinancials
	DROP TABLE #ProjectExpenses
	DROP TABLE #ActualAndProjectedValuesDaily
	DROP TABLE #ActualAndProjectedValuesMonthly
	DROP TABLE #ActualTimeEntries
	DROP TABLE #CTE
	DROP TABLE #cteFinancialsRetrospectiveActualHours
	DROP TABLE #MileStoneEntries1
	DROP TABLE #MilestoneRevenueRetrospective
	DROP TABLE #MonthlyHours

END
	

