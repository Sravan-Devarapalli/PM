CREATE PROCEDURE [dbo].[GetWeeklyRevenuesForProject]
(
	@ProjectId INT,
	@StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
	@Step INT = 7,
	@ActualsEndDate DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @Today DATETIME, 
			@ProjectIdLocal INT, 
			@startDateLocal DATETIME, 
			@endDateLocal DATETIME, 
			@ActualsEndDateLocal DATETIME, 
			@CurrentMonthEnd DATETIME,
			@ActualTimeEntryEndDate DATETIME,
			@LastMonthEnd DATETIME

	SELECT @Today = CONVERT(DATE, dbo.GettingPMTime(GETUTCDATE()))
	SELECT @ProjectIdLocal =@ProjectId,
			@startDateLocal= @StartDate,
			@endDateLocal =@EndDate,
			@ActualsEndDateLocal=@ActualsEndDate
	SELECT @CurrentMonthEnd =EOMONTH ( @Today )
	SELECT @LastMonthEnd=convert (date,DATEADD(MONTH, DATEDIFF(MONTH, -1, @Today)-1, -1))

	iF(@startDateLocal IS NULL)
	BEGIN
	 SELECT @startDateLocal= p.StartDate, @endDateLocal=p.EndDate
	 FROM Project P
	 WHERE P.ProjectId=@ProjectIdLocal
	END
	
	 SELECT r.* INTO #Ranges
	 FROM	(SELECT  c.MonthStartDate as StartDate,c.MonthEndDate  AS EndDate
		FROM dbo.Calendar c
		WHERE c.date BETWEEN @startDateLocal and @endDateLocal
		AND @Step = 30
		GROUP BY c.MonthStartDate,c.MonthEndDate  
		UNION ALL
		SELECT  c.date,c.date + 6
		FROM dbo.Calendar c
		WHERE c.date BETWEEN @startDateLocal and @endDateLocal
		AND DATEDIFF(day,@startDateLocal,c.date) % 7 = 0
		AND @Step = 7
		UNION ALL
		SELECT  c.date,c.date
		FROM dbo.Calendar c
		WHERE c.date BETWEEN @startDateLocal and @endDateLocal
		AND @Step = 1
	)  r


		SELECT CC.ProjectId,
				TE.PersonId,
				TE.ChargeCodeDate,
				SUM(CASE WHEN TEH.IsChargeable = 1 THEN TEH.ActualHours ELSE 0 END) BillableHOursPerDay,
				SUM(CASE WHEN TEH.IsChargeable = 0 THEN TEH.ActualHours ELSE 0 END) NonBillableHoursPerDay,
				P.IsHourlyAmount
		INTO #ActualTimeEntries
		FROM TimeEntry TE
		JOIN TimeEntryHours TEH ON TEH.TimeEntryId = TE.TimeEntryId
		JOIN ChargeCode CC on CC.Id = TE.ChargeCodeId 
		JOIN (
				SELECT Pro.ProjectId,CAST(CASE WHEN SUM(CAST(m.IsHourlyAmount as INT)) > 0 THEN 1 ELSE 0 END AS BIT) AS IsHourlyAmount
				FROM Project Pro 
					LEFT JOIN Milestone m ON m.ProjectId = Pro.ProjectId 
				WHERE Pro.IsAllowedToShow = 1 AND Pro.ProjectId =@ProjectIdLocal
				GROUP BY Pro.ProjectId
			 ) P ON p.ProjectId = CC.ProjectId
		WHERE TE.ChargeCodeDate>=@startDateLocal AND TE.ChargeCodeDate<=@endDateLocal AND (@ActualsEndDateLocal IS NULL OR TE.ChargeCodeDate<= @ActualsEndDateLocal)
		GROUP BY CC.ProjectId, TE.PersonId, TE.ChargeCodeDate,P.IsHourlyAmount

		select @ActualTimeEntryEndDate = MAX(ChargeCodeDate) FROM #ActualTimeEntries
	
	
		SELECT  m.ProjectId,
				m.[MilestoneId],
				mp.PersonId,
				cal.Date,
				MPE.Id,
				MPE.Amount,
				m.IsHourlyAmount,	
				m.IsDefault,
				SUM(mpe.HoursPerDay) AS ActualHoursPerDay,
				SUM(CONVERT(DECIMAL(4,2),CASE WHEN cal.DayOff = 0  THEN mpe.HoursPerDay -- No Time-off and no company holiday
					WHEN ( cal.companydayoff = 1) OR ( cal.companydayoff = 0 AND ISNULL(cal.TimeoffHours,8) = 8) THEN 0 -- only company holiday OR person complete dayoff
					ELSE mpe.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off
				END)) AS HoursPerDay,
				SUM(CONVERT(DECIMAL(4,2),CASE WHEN cal.DayOff = 0  THEN mpe.HoursPerDay -- No Time-off and no company holiday
					WHEN ( cal.companydayoff = 1) OR ( cal.companydayoff = 0 AND ISNULL(cal.TimeoffHours,8) = 8) THEN 0 -- only company holiday OR person complete dayoff
					ELSE mpe.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off
				END) * mpe.Amount) AS PersonMilestoneDailyAmount--PersonLevel
		INTO #MileStoneEntries
		FROM dbo.Project P 
		INNER JOIN dbo.[Milestone] AS m ON P.ProjectId=m.ProjectId AND p.IsAllowedToShow = 1 AND p.projectid != 174
		INNER JOIN dbo.MilestonePerson AS mp ON m.[MilestoneId] = mp.[MilestoneId]
		INNER JOIN dbo.MilestonePersonEntry AS mpe ON mp.MilestonePersonId = mpe.MilestonePersonId
		INNER JOIN dbo.PersonCalendarAuto AS cal ON cal.Date BETWEEN mpe.Startdate AND mpe.EndDate AND cal.PersonId = mp.PersonId 
		WHERE P.ProjectId =@ProjectIdLocal AND cal.Date BETWEEN mpe.Startdate AND mpe.EndDate AND cal.PersonId = mp.PersonId 
		GROUP BY  m.ProjectId,m.[MilestoneId],mp.PersonId,cal.Date,m.IsHourlyAmount ,m.IsDefault,MPE.Id,MPE.Amount
	
		SELECT s.Date, s.MilestoneId, SUM(HoursPerDay) AS HoursPerDay
		INTO #CTE
		FROM #MileStoneEntries AS s
		WHERE s.IsHourlyAmount = 0
		GROUP BY s.Date, s.MilestoneId
	
	
	
		SELECT C.MonthStartDate, C.MonthEndDate,C.MonthNumber, s.MilestoneId, SUM(HoursPerDay) AS HoursPerMonth
		INTO #MonthlyHours
		FROM dbo.v_MilestonePersonSchedule AS s
		INNER JOIN dbo.Calendar C ON C.Date = s.Date 
		WHERE s.IsHourlyAmount = 0
		GROUP BY s.MilestoneId, C.MonthStartDate, C.MonthEndDate,C.MonthNumber
	



	SELECT m.* INTO  #MilestoneRevenueRetrospective
	FROM 
		 (SELECT --Milestones with a fixed amount and monthly revenues
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
				ISNULL((m.Amount/ NULLIF(MTHours.TotalHours,0))* d.HoursPerDay,0) AS MilestoneDailyAmount ,
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
		FROM #MileStoneEntries mp
				INNER JOIN dbo.Project AS p ON mp.ProjectId = p.ProjectId AND mp.IsHourlyAmount = 1
		GROUP BY mp.MilestoneId, mp.Date, mp.IsHourlyAmount
	) as m

	
	
		SELECT	pro.ProjectId,
			Per.PersonId,
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
			FULL JOIN #MileStoneEntries AS ME ON ME.ProjectId = AE.ProjectId AND AE.PersonId = ME.PersonId AND ME.Date = AE.ChargeCodeDate 
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


	
		SELECT f.ProjectId,
			   f.Date, 
			   f.PersonMilestoneDailyAmount,
			   f.PersonDiscountDailyAmount,
			   (ISNULL(f.PayRate, 0) + ISNULL(f.OverheadRate, 0)+ISNULL(f.BonusRate,0)+ISNULL(f.VacationRate,0)) SLHR,
			   ISNULL(f.PayRate,0) PayRate,
			   f.MLFOverheadRate,
			   f.PersonHoursPerDay,
			   f.PersonId,
			   f.Discount,
			   f.IsHourlyAmount,
			   f.BillableHOursPerDay,
  			   f.NonBillableHoursPerDay,
			   f.ActualHoursPerDay,
			   f.BillRate
		INTO #FinancialsRetro
		FROM #cteFinancialsRetrospectiveActualHours f
		WHERE f.Date BETWEEN @startDateLocal AND @endDateLocal
	
		SELECT	f.ProjectId,
				f.Date,
				f.PersonId,
				SUM(f.PersonMilestoneDailyAmount) AS ProjectedRevenueperDay,
				SUM(f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount) AS ProjectedRevenueNet,
				SUM(f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount - (
																					CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate THEN f.SLHR 
																					ELSE f.PayRate + f.MLFOverheadRate END
																				) * ISNULL(f.PersonHoursPerDay, 0)) AS  ProjectedGrossMargin,
				SUM(CASE WHEN f.IsHourlyAmount = 0 THEN f.PersonMilestoneDailyAmount ELSE 0 END ) AS FixedActualRevenuePerDay,
				(ISNULL(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.BillRate* f.ActualHoursPerDay ELSE 0 END),0) / ISNULL(NULLIF(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.ActualHoursPerDay ELSE 0 END),0),1)) * MAX(CASE WHEN f.IsHourlyAmount = 1 THEN  f.BillableHOursPerDay ELSE 0 END) HourlyActualRevenuePerDay,

				 ISNULL(SUM(CASE WHEN (f.IsHourlyAmount = 0 and  (@ActualsEndDateLocal IS NULL OR f.Date<= EOMONTH(@ActualsEndDateLocal))) THEN f.PersonMilestoneDailyAmount ELSE 0 END),0)- (
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
				
				SUM(CASE WHEN f.IsHourlyAmount = 0 AND @ActualsEndDateLocal IS NOT NULL AND f.Date > EOMONTH(@ActualsEndDateLocal) THEN f.PersonMilestoneDailyAmount 
						 WHEN f.IsHourlyAmount = 1 AND (f.Date > @Today AND f.BillableHOursPerDay IS NULL AND f.NonBillableHoursPerDay IS NULL) THEN f.PersonMilestoneDailyAmount
						 ELSE 0 END) AS RemainingProjectedRevenuePerDay,
				ISNULL(SUM(f.PersonHoursPerDay), 0) AS ProjectedHoursPerDay,
				SUM(f.BillableHOursPerDay + f.NonBillableHoursPerDay) as ActualHoursPerDay,
				ISNULL(SUM(CASE WHEN (f.Date > @Today AND f.BillableHOursPerDay IS NULL AND f.NonBillableHoursPerDay IS NULL) THEN f.PersonHoursPerDay ELSE 0 END),0) as RemianingProjectedHoursPerDay,
		  		min(f.Discount) as Discount
		INTO #ActualAndProjectedValuesDaily
		FROM #FinancialsRetro AS f
		GROUP BY f.ProjectId,f.PersonId, f.Date
	

		SELECT pexp.ProjectId,
			SUM(pexp.EstimatedAmount) as EstimatedAmount,
			SUM(pexp.EstimatedReimbursement) as EstimatedReimbursement,
			SUM(CASE WHEN @ActualsEndDateLocal IS NULL OR pexp.date<= @ActualsEndDateLocal THEN  pexp.ActualExpense ELSE 0 END) as ActualExpense,
			SUM(CASE WHEN @ActualsEndDateLocal IS NULL OR pexp.date<= @ActualsEndDateLocal THEN  pexp.ActualReimbursement ELSE 0 END) as ActualReimbursement,
			r.StartDate,
			r.EndDate
		INTO #ProjectExpensesMonthly
		FROM v_ProjectDailyExpenses as pexp
		INNER JOIN #Ranges r on pexp.Date>=r.StartDate AND pexp.Date <= r.EndDate
		WHERE pexp.ProjectId =@ProjectIdLocal AND pexp.Date BETWEEN @startDateLocal AND @endDateLocal
		GROUP BY pexp.ProjectId, r.StartDate,  r.EndDate
	

		SELECT CT.ProjectId, 
				r.StartDate,
				r.EndDate,
				SUM(ISNULL(CT.ProjectedRevenueperDay, 0)) AS ProjectedRevenue,
				SUM(ISNULL(CT.ProjectedRevenueNet, 0)) as ProjectedRevenueNet,
				SUM(ISNULL(CT.ProjectedGrossMargin, 0)) as ProjectedGrossMargin,
				SUM(ISNULL(CT.HourlyActualRevenuePerDay, 0) + CASE WHEN @ActualsEndDateLocal IS NULL OR CT.Date<= EOMONTH(@ActualsEndDateLocal) THEN ISNULL(CT.FixedActualRevenuePerDay, 0) ELSE 0 END) AS ActualRevenue,
				SUM(ISNULL(CT.FixedActualMarginPerDay,0)+ISNULL(HourlyActualMarginPerDay,0)) as ActualMargin,
				SUM(ISNULL(CT.RemainingProjectedRevenuePerDay,0) + ISNULL(CT.HourlyActualRevenuePerDay, 0)+ CASE WHEN @ActualsEndDateLocal IS NULL OR CT.Date<= EOMONTH(@ActualsEndDateLocal) THEN ISNULL(CT.FixedActualRevenuePerDay, 0) ELSE 0 END) as EACRevenue,
				SUM(CT.ProjectedHoursPerDay) as ProjectedHours,
				SUM(CT.ActualHoursPerDay) as ActualHours,
				SUM(CT.RemianingProjectedHoursPerDay) as RemianingProjectedHours,
				MAX(ISNULL(CT.Discount, 0)) Discount
		INTO #ActualAndProjectedValuesMonthly
		FROM #ActualAndProjectedValuesDaily CT
		INNER JOIN #Ranges r on CT.Date>=r.StartDate AND CT.Date <= r.EndDate
		GROUP BY CT.ProjectId, r.StartDate, r.EndDate

		CREATE TABLE #BudgetRestrospective
		(
			ProjectId INT,
		    Date DATETIME, 
		    PersonMilestoneDailyAmount DECIMAL(18,2),
			PersonDiscountDailyAmount DECIMAL(18,2),
			SLHR DECIMAL(10,2),
			PayRate DECIMAL(10,2),
			MLFOverheadRate DECIMAL(10,2),
			PersonHoursPerDay DECIMAL(4,2),
			PersonId INT,
			Discount INT,
			IsHourlyAmount BIT,
		    ActualHoursPerDay DECIMAL(4,2),
		    BillRate DECIMAL(18,2)
		)

		INSERT INTO #BudgetRestrospective
		(
			ProjectId ,
		    Date , 
		    PersonMilestoneDailyAmount ,
			PersonDiscountDailyAmount ,
			SLHR ,
			PayRate ,
			MLFOverheadRate ,
			PersonHoursPerDay ,
			PersonId ,
			Discount ,
			IsHourlyAmount ,
		    ActualHoursPerDay ,
		    BillRate 
		)
		EXEC [dbo].[FinancialsRetrospectiveBudget] @ProjectId = @ProjectIdLocal
		

		  SELECT f.ProjectId,
			   r.StartDate,
			   r.EndDate,
			   SUM(f.PersonMilestoneDailyAmount) AS Revenue
		  INTO #BudgetValuesMonthly
 		  FROM #BudgetRestrospective AS f
		  INNER JOIN #Ranges r on f.Date>=r.StartDate AND f.Date <= r.EndDate
		  where f.ProjectId = @ProjectIdLocal
		  GROUP BY f.ProjectId, r.StartDate, r.EndDate
	
		SELECT
			ISNULL(APV.ProjectId,PEM.ProjectId) ProjectId,
			ISNULL(APV.StartDate,PEM.StartDate) StartDate,
			ISNULL(APV.EndDate,PEM.EndDate) EndDate,
			CONVERT(DECIMAL(18,2),ISNULL(APV.ProjectedRevenue,0)) AS 'Revenue',
			CONVERT(DECIMAL(18,2),ISNULL(APV.ProjectedRevenueNet,0)+ISNULL(PEM.EstimatedReimbursement,0)) as 'RevenueNet',
			CONVERT(DECIMAL(18,2),ISNULL(APV.ProjectedGrossMargin,0)+(ISNULL(PEM.EstimatedReimbursement,0)-ISNULL(PEM.EstimatedAmount,0)))  as 'GrossMargin',
			CONVERT(DECIMAL(18,2), ISNULL(APV.ActualRevenue,0)) ActualRevenue,
			CONVERT(DECIMAL(18,2), ISNULL(APV.ActualMargin,0)+(ISNULL(PEM.ActualReimbursement,0)-ISNULL(PEM.ActualExpense,0))) ActualMargin,
			CONVERT(DECIMAL(18,2),ISNULL(APV.EACRevenue,0)) EACRevenue,
			CONVERT(DECIMAL(18,2),ISNULL(B.Revenue,0)) AS BudgetRevenue,
			CONVERT(DECIMAL(6,2),ISNULL(APV.ProjectedHours,0)) as ProjectedHours,
			CONVERT(DECIMAL(6,2),ISNULL(APV.ActualHours,0)) as ActualHours,
			CONVERT(DECIMAL(6,2),ISNULL(APV.RemianingProjectedHours,0)+ISNULL(APV.ActualHours,0))  as EACHours,
			ROW_NUMBER() OVER(ORDER BY ISNULL(APV.StartDate,PEM.StartDate)) rownum
		INTO #WeeklyRevenue
		FROM #ActualAndProjectedValuesMonthly APV
		LEFT JOIN #BudgetValuesMonthly B on B.ProjectId = APV.ProjectId AND APV.StartDate = B.StartDate  AND APV.EndDate = B.EndDate
		FULL JOIN #ProjectExpensesMonthly PEM 
		ON PEM.ProjectId = APV.ProjectId AND APV.StartDate = PEM.StartDate  AND APV.EndDate = PEM.EndDate
		ORDER BY StartDate

		IF(@ActualsEndDateLocal IS NULL)
		BEGIN
			IF EXISTS(SELECT 1 FROM #FinancialsRetro WHERE IsHourlyAmount = 0)
			BEGIN
				SELECT @ActualsEndDateLocal = CASE WHEN EOMONTH(@ActualsEndDateLocal) <=@ActualTimeEntryEndDate and @ActualTimeEntryEndDate IS not null THEN @ActualTimeEntryEndDate ELSE EOMONTH(@ActualsEndDateLocal) END
			END
			ELSE
			BEGIN
				SELECT @ActualsEndDateLocal = @ActualTimeEntryEndDate
			END
		END


		SELECT c1.ProjectId,
		       c1.StartDate,
		       c1.EndDate,
		       SUM(c2.Revenue)AS Revenue, 
			   SUM(c2.RevenueNet) as RevenueNet, 
			   SUM(c2.GrossMargin) as GrossMargin, 
			   CASE WHEN (@ActualsEndDateLocal IS NULL OR c1.StartDate<=EOMONTH(@ActualsEndDateLocal)) THEN SUM(c2.ActualRevenue)
					ELSE 0 END as ActualRevenue ,
			   SUM(c2.ActualMargin) as ActualMargin,
		       SUM(c2.EACRevenue) as EACRevenue, 
			   SUM(c2.BudgetRevenue) as BudgetRevenue,
		       SUM(c2.ProjectedHours) as ProjectedHours,
		       SUM(c2.ActualHours) as ActualHours,
		       SUM(c2.EACHours) as EACHours
	    FROM #WeeklyRevenue c2 ,  #WeeklyRevenue c1  
		WHERE c2.rownum <= c1.rownum
		GROUP BY c1.ProjectId,
		c1.StartDate,
		c1.EndDate

	DROP TABLE #Ranges
	DROP TABLE #ActualTimeEntries
	DROP TABLE #MileStoneEntries
	DROP TABLE #CTE
	DROP TABLE #MonthlyHours
	DROP TABLE #MilestoneRevenueRetrospective
	DROP TABLE #cteFinancialsRetrospectiveActualHours
	DROP TABLE #FinancialsRetro
	DROP TABLE #ActualAndProjectedValuesDaily
	DROP TABLE #ProjectExpensesMonthly
	DROP TABLE #ActualAndProjectedValuesMonthly
	DROP TABLE #BudgetValuesMonthly
	DROP TABLE #WeeklyRevenue
	DROP TABLE #BudgetRestrospective
END

