CREATE PROCEDURE [dbo].[GetBurnReportForProject]
(
	@ProjectId   INT,
	@StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
	@ActualsEndDate DATETIME = NULL
)
AS
BEGIN
	DECLARE @Today DATETIME, 
			@ProjectIdLocal INT, 
			@startDateLocal DATETIME, 
			@endDateLocal DATETIME, 
			@ActualsEndDateLocal DATETIME, 
			@LastMonthEnd DATETIME,
			@ProjRemainingDate DATETIME

	SELECT @Today = CONVERT(DATE, dbo.GettingPMTime(GETUTCDATE()))
	SELECT @ProjectIdLocal =@ProjectId,
			@startDateLocal= @StartDate,
			@endDateLocal =@EndDate,
			@ActualsEndDateLocal=@ActualsEndDate
	   --SELECT @CurrentMonthEnd =EOMONTH ( @Today )
	SELECT @LastMonthEnd=convert (date,DATEADD(MONTH, DATEDIFF(MONTH, -1, @Today)-1, -1))

	IF(@startDateLocal IS NULL)
	BEGIN
	 SELECT @startDateLocal= p.StartDate, @endDateLocal=p.EndDate
	 FROM Project P
	 WHERE P.ProjectId=@ProjectIdLocal
	END

	SELECT @ProjRemainingDate = CASE WHEN @ActualsEndDateLocal IS NULL THEN @Today ELSE @ActualsEndDateLocal END

;WITH ActualTimeEntries
AS 
(
	SELECT CC.ProjectId,
			TE.PersonId,
			TE.ChargeCodeDate,
			SUM(CASE WHEN TEH.IsChargeable = 1 THEN TEH.ActualHours ELSE 0 END) BillableHOursPerDay,
			SUM(CASE WHEN TEH.IsChargeable = 0 THEN TEH.ActualHours ELSE 0 END) NonBillableHoursPerDay,
			P.IsHourlyAmount
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
	WHERE TE.ChargeCodeDate>=@startDateLocal AND TE.ChargeCodeDate<=@endDateLocal AND (@ActualsEndDateLocal IS NULL OR TE.ChargeCodeDate<= @ActualsEndDateLocal)
	GROUP BY CC.ProjectId, TE.PersonId, TE.ChargeCodeDate,P.IsHourlyAmount
),
MileStoneEntries
AS
(
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
				WHEN (cal.companydayoff = 1) OR (cal.companydayoff = 0 AND ISNULL(cal.TimeoffHours,8) = 8) THEN 0 -- only company holiday OR person complete dayoff
				ELSE mpe.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off
			END)) AS HoursPerDay,
			SUM(CONVERT(DECIMAL(4,2),CASE WHEN cal.DayOff = 0  THEN mpe.HoursPerDay -- No Time-off and no company holiday
				WHEN (cal.companydayoff = 1) OR ( cal.companydayoff = 0 AND ISNULL(cal.TimeoffHours,8) = 8) THEN 0 -- only company holiday OR person complete dayoff
				ELSE mpe.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off
			END) * mpe.Amount) AS PersonMilestoneDailyAmount--PersonLevel
			FROM dbo.Project P 
			INNER JOIN dbo.[Milestone] AS m ON P.ProjectId=m.ProjectId AND p.IsAllowedToShow = 1 AND p.projectid != 174
			INNER JOIN dbo.MilestonePerson AS mp ON m.[MilestoneId] = mp.[MilestoneId]
			INNER JOIN dbo.MilestonePersonEntry AS mpe ON mp.MilestonePersonId = mpe.MilestonePersonId
			INNER JOIN dbo.PersonCalendarAuto AS cal ON cal.Date BETWEEN mpe.Startdate AND mpe.EndDate AND cal.PersonId = mp.PersonId 
			WHERE P.ProjectId =@ProjectIdLocal
		    GROUP BY  m.ProjectId,m.[MilestoneId],mp.PersonId,cal.Date,m.IsHourlyAmount ,m.IsDefault,MPE.Id,MPE.Amount
),
CTE 
AS 
(
	SELECT s.Date, s.MilestoneId, SUM(HoursPerDay) AS HoursPerDay
	FROM MileStoneEntries AS s
	WHERE s.IsHourlyAmount = 0
	GROUP BY s.Date, s.MilestoneId
),

MonthlyHours
AS
(
	SELECT C.MonthStartDate, C.MonthEndDate,C.MonthNumber, s.MilestoneId, SUM(HoursPerDay) AS HoursPerMonth
	FROM dbo.v_MilestonePersonSchedule AS s
	INNER JOIN dbo.Calendar C ON C.Date = s.Date 
	WHERE s.IsHourlyAmount = 0
	GROUP BY s.MilestoneId, C.MonthStartDate, C.MonthEndDate,C.MonthNumber
),

MilestoneRevenueRetrospective
AS
(
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
		JOIN MonthlyHours MH on MH.milestoneid=M.MilestoneId AND cal.Date BETWEEN MH.MonthStartDate AND MH.MonthEndDate
		INNER JOIN CTE AS d ON d.date = cal.Date and m.MilestoneId = d.MileStoneId
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
							FROM CTE AS s 
							GROUP BY s.MilestoneId
						) AS MTHours  ON MTHours.MilestoneId  = m.MilestoneId
			INNER JOIN CTE AS d ON d.date = cal.Date and m.MilestoneId = d.MileStoneId
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
		FROM MileStoneEntries mp
			INNER JOIN dbo.Project AS p ON mp.ProjectId = p.ProjectId AND mp.IsHourlyAmount = 1
	GROUP BY mp.MilestoneId, mp.Date, mp.IsHourlyAmount
),
cteFinancialsRetrospectiveActualHours
as
(
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
FROM ActualTimeEntries AS AE --ActualEntriesByPerson
		FULL JOIN MileStoneEntries AS ME ON ME.ProjectId = AE.ProjectId AND AE.PersonId = ME.PersonId AND ME.Date = AE.ChargeCodeDate 
		INNER JOIN dbo.Person Per ON per.PersonId = ISNULL(ME.PersonId,AE.PersonId)
		INNER JOIN dbo.Project Pro ON Pro.ProjectId = ISNULL(ME.ProjectId,AE.ProjectId) 
		INNER JOIN dbo.Calendar C ON c.Date = ISNULL(ME.Date,AE.ChargeCodeDate)
		INNER JOIN dbo.GetFutureDateTable() FD ON 1=1 --For improving query performance we are using table valued function instead of scalar function.
		LEFT JOIN dbo.[v_PersonPayRetrospective] AS p ON p.PersonId = per.PersonId AND p.Date = c.Date
		LEFT JOIN v_MLFOverheadFixedRateTimescale MLFO ON MLFO.TimescaleId = p.Timescale AND c.Date BETWEEN MLFO.StartDate AND ISNULL(MLFO.EndDate,FD.FutureDate)
		LEFT JOIN dbo.v_OverheadFixedRateTimescale AS o ON p.Date BETWEEN o.StartDate AND ISNULL(o.EndDate, FD.FutureDate) AND o.TimescaleId = p.Timescale
		LEFT JOIN MilestoneRevenueRetrospective AS r ON ME.MilestoneId = r.MilestoneId AND c.Date = r.Date
	GROUP BY pro.ProjectId,Per.PersonId,c.Date,C.DaysInYear,AE.BillableHOursPerDay,AE.NonBillableHoursPerDay,ME.IsHourlyAmount,ME.HoursPerDay,ME.PersonMilestoneDailyAmount,
			p.Timescale,p.HourlyRate,p.BonusRate,p.VacationDays,
			r.HoursPerDay,r.MilestoneDailyAmount,r.Discount,MLFO.Rate,ME.Amount,ME.Id,ME.ActualHoursPerDay,AE.IsHourlyAmount
),

FinancialsRetro AS 
	(
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
	FROM cteFinancialsRetrospectiveActualHours f
	WHERE f.ProjectId=@ProjectIdLocal and f.Date >= @startDateLocal AND f.Date <= @endDateLocal
	),


		  -- Actuals 

		  ActualsAndProjectedValuesDaily
		  AS
		  (
		  SELECT	f.ProjectId,
					f.PersonId,
					f.Date,
					SUM(CASE WHEN f.IsHourlyAmount = 1 THEN (CASE WHEN @ActualsEndDateLocal IS NULL THEN f.PersonMilestoneDailyAmount
																  WHEN f.Date<=	@ActualsEndDateLocal THEN f.PersonMilestoneDailyAmount ELSE 0 END)
						     WHEN f.IsHourlyAmount = 0 THEN (CASE WHEN @ActualsEndDateLocal IS NULL THEN f.PersonMilestoneDailyAmount
							             						  WHEN f.Date<=	EOMONTH(@ActualsEndDateLocal) THEN f.PersonMilestoneDailyAmount ELSE 0 END )
							 ELSE 0 END) AS ProjectedRevenueperDay,

					SUM(CASE WHEN f.IsHourlyAmount = 1 THEN (CASE WHEN @ActualsEndDateLocal IS NULL THEN (f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount - (
																					CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate THEN f.SLHR 
																					ELSE f.PayRate + f.MLFOverheadRate END
																				) * ISNULL(f.PersonHoursPerDay, 0)) 
																  WHEN f.Date<=	@ActualsEndDateLocal THEN (f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount - (
																					CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate THEN f.SLHR 
																					ELSE f.PayRate + f.MLFOverheadRate END
																				) * ISNULL(f.PersonHoursPerDay, 0))  ELSE 0 END)
						     WHEN f.IsHourlyAmount = 0 THEN (CASE WHEN @ActualsEndDateLocal IS NULL THEN (f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount - (
																					CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate THEN f.SLHR 
																					ELSE f.PayRate + f.MLFOverheadRate END
																				) * ISNULL(f.PersonHoursPerDay, 0)) 
							             						  WHEN f.Date<=	EOMONTH(@ActualsEndDateLocal) THEN (f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount - (
																					CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate THEN f.SLHR 
																					ELSE f.PayRate + f.MLFOverheadRate END
																				) * ISNULL(f.PersonHoursPerDay, 0))  ELSE 0 END )
							 ELSE 0 END) AS ProjectedGrossMargin,

					SUM(CASE WHEN f.IsHourlyAmount = 0 THEN f.PersonMilestoneDailyAmount ELSE 0 END ) AS FixedActualRevenuePerDay,
					(ISNULL(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.BillRate* f.ActualHoursPerDay ELSE 0 END),0) / ISNULL(NULLIF(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.ActualHoursPerDay ELSE 0 END),0),1)) * MAX(CASE WHEN f.IsHourlyAmount = 1 THEN  f.BillableHOursPerDay ELSE 0 END) HourlyActualRevenuePerDay,
					--SUM(CASE WHEN (f.IsHourlyAmount = 0 and (@ActualsEndDateLocal IS NULL OR f.Date<=@ActualsEndDateLocal)) THEN 	f.PersonMilestoneDailyAmount ELSE 0 END)- (
								(ISNULL( MAX( CASE WHEN f.IsHourlyAmount = 0 THEN  f.BillableHOursPerDay + f.NonBillableHoursPerDay ELSE 0 END ),0) * 
								ISNULL( CASE WHEN ISNULL(SUM(CASE WHEN f.IsHourlyAmount = 0 THEN f.ActualHoursPerDay ELSE 0 END),0) > 0 
											THEN SUM((CASE WHEN f.SLHR >=  f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END)* CASE WHEN f.IsHourlyAmount = 0 THEN f.ActualHoursPerDay ELSE 0 END) / SUM(CASE WHEN f.IsHourlyAmount = 0 THEN f.ActualHoursPerDay ELSE 0 END)
											ELSE SUM((CASE WHEN f.SLHR >=  f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END)) / ISNULL(COUNT(f.PayRate),1)  
										END ,0)
							)
							 AS FixedActualCostPerDay,
						--((ISNULL(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.BillRate* f.ActualHoursPerDay ELSE 0 END),0) / ISNULL(NULLIF(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.ActualHoursPerDay ELSE 0 END),0),1)) * MAX(CASE WHEN f.IsHourlyAmount = 1 THEN  f.BillableHOursPerDay ELSE 0 END))
						-- -  (
								(MAX( CASE WHEN f.IsHourlyAmount = 1 THEN  f.BillableHOursPerDay + f.NonBillableHoursPerDay ELSE 0 END ) * 
								ISNULL( CASE WHEN ISNULL(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.ActualHoursPerDay ELSE 0 END),0) > 0 
											THEN SUM((CASE WHEN f.SLHR >=  f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END)* CASE WHEN f.IsHourlyAmount = 1 THEN f.ActualHoursPerDay ELSE 0 END) / SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.ActualHoursPerDay ELSE 0 END)
											ELSE SUM((CASE WHEN f.SLHR >=  f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END)) / ISNULL(COUNT(f.PayRate),1)  
										END ,0)
							) AS HourlyActualCostPerDay,
					SUM(ISNULL(f.BillableHOursPerDay, 0)+ ISNULL(f.NonBillableHoursPerDay,0)) AS ActualHours,
		  			SUM(CASE WHEN @ActualsEndDateLocal IS NULL THEN ISNULL(f.PersonHoursPerDay,0) 
							 WHEN f.Date<= @ActualsEndDateLocal THEN ISNULL(f.PersonHoursPerDay,0) ELSE 0 END) as ProjectedHours,
					SUM(ISNULL(CASE WHEN (f.Date > @Today AND f.BillableHOursPerDay IS NULL AND f.NonBillableHoursPerDay IS NULL) THEN f.PersonHoursPerDay ELSE 0 END, 0)) as RemainingProjectedHours,
					SUM(CASE WHEN f.IsHourlyAmount = 0 AND f.Date > EOMONTH(@ProjRemainingDate) THEN f.PersonMilestoneDailyAmount 
							 WHEN f.IsHourlyAmount = 1 AND (f.Date > @ProjRemainingDate AND f.BillableHOursPerDay IS NULL AND f.NonBillableHoursPerDay IS NULL) THEN f.PersonMilestoneDailyAmount
							 ELSE 0 END) AS RemainingProjectedRevenue,
					SUM(CASE WHEN f.IsHourlyAmount = 0 AND f.BillableHOursPerDay IS NULL AND f.NonBillableHoursPerDay IS NULL AND f.Date > @ProjRemainingDate THEN (CASE WHEN f.Date > EOMONTH(@ProjRemainingDate) THEN (f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount) ELSE 0 END - (
																					CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate THEN f.SLHR 
																					ELSE f.PayRate + f.MLFOverheadRate END
																				) * ISNULL(f.PersonHoursPerDay, 0)) 
							 WHEN f.IsHourlyAmount = 1 AND (f.Date > @ProjRemainingDate AND f.BillableHOursPerDay IS NULL AND f.NonBillableHoursPerDay IS NULL) THEN (f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount - (
																					CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate THEN f.SLHR 
																					ELSE f.PayRate + f.MLFOverheadRate END
																				) * ISNULL(f.PersonHoursPerDay, 0)) 
							ELSE 0 END) AS RemainingProjectedMargin
		FROM FinancialsRetro AS f
		WHERE f.ProjectId=@ProjectIdLocal
		GROUP BY f.ProjectId, f.PersonId,f.Date
		  )

	
	   SELECT	CT.ProjectId,
				CT.PersonId,
				SUM(ISNULL(CT.ProjectedRevenueperDay,0)) as ProjectedRevenue,
				SUM(ISNULL(CT.ProjectedGrossMargin,0)) as ProjectedMargin, 
				SUM(CT.ProjectedHours) as ProjectedHours,
				SUM(ISNULL(CT.HourlyActualRevenuePerDay, 0) + CASE WHEN CT.Date<= EOMONTH(@ProjRemainingDate) THEN  ISNULL(CT.FixedActualRevenuePerDay, 0) ELSE 0 END) AS ActualRevenue,
				SUM(ISNULL(CT.HourlyActualRevenuePerDay, 0)- ISNULL(CT.HourlyActualCostPerDay,0) + CASE WHEN CT.Date<= EOMONTH(@ProjRemainingDate) THEN  ISNULL(CT.FixedActualRevenuePerDay, 0) ELSE 0 END - ISNULL(CT.FixedActualCostPerDay,0)) as ActualMargin,
				SUM(CT.ActualHours) as ActualHours,
				SUM(ISNULL(CT.RemainingProjectedRevenue,0)) as RemainingProjectedRevenue,
				SUM(ISNULL(CT.RemainingProjectedMargin,0)) as RemainingProjectedMargin,
				SUM(CT.RemainingProjectedHours) as RemainingProjectedHours,
				SUM(ISNULL(CT.RemainingProjectedRevenue,0)+ ISNULL(CT.HourlyActualRevenuePerDay, 0) + CASE WHEN CT.Date<= EOMONTH(@ProjRemainingDate) THEN  ISNULL(CT.FixedActualRevenuePerDay, 0) ELSE 0 END) as EACRevenue,
				SUM(ISNULL(CT.RemainingProjectedMargin,0) + ISNULL(CT.HourlyActualRevenuePerDay, 0)- ISNULL(CT.HourlyActualCostPerDay,0) + CASE WHEN CT.Date<= EOMONTH(@ProjRemainingDate) THEN  ISNULL(CT.FixedActualRevenuePerDay, 0) ELSE 0 END - ISNULL(CT.FixedActualCostPerDay,0)) as EACMargin,
				SUM(CT.ActualHours + CT.RemainingProjectedHours) as EACHours
		INTO #ActualsAndProjected
		FROM ActualsAndProjectedValuesDaily CT
		GROUP BY CT.ProjectId, CT.PersonId

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
	

		   SELECT  f.ProjectId,
				   f.PersonId,
				   ISNULL(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN (CASE WHEN @ActualsEndDateLocal IS NULL THEN f.PersonMilestoneDailyAmount
																  WHEN f.Date<=	@ActualsEndDateLocal THEN f.PersonMilestoneDailyAmount ELSE 0 END)
						     WHEN f.IsHourlyAmount = 0 THEN (CASE WHEN @ActualsEndDateLocal IS NULL  THEN f.PersonMilestoneDailyAmount
							             						  WHEN f.Date<=	EOMONTH(@ActualsEndDateLocal) THEN f.PersonMilestoneDailyAmount ELSE 0 END )
							 ELSE 0 END),0) AS Revenue,

				   ISNULL(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN (CASE WHEN @ActualsEndDateLocal IS NULL THEN (f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount -
																												(CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate 
																													  THEN f.SLHR  ELSE f.PayRate+f.MLFOverheadRate END) 
																												*ISNULL(f.PersonHoursPerDay, 0))
																		WHEN f.Date<=	@ActualsEndDateLocal THEN (f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount -
																													(CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate 
																															THEN f.SLHR  ELSE f.PayRate+f.MLFOverheadRate END) 
																													*ISNULL(f.PersonHoursPerDay, 0)) ELSE 0 END)
								   WHEN f.IsHourlyAmount = 0 THEN (CASE WHEN @ActualsEndDateLocal IS NULL  THEN (f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount -
																																		(CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate 
																																			  THEN f.SLHR  ELSE f.PayRate+f.MLFOverheadRate END) 
																																		*ISNULL(f.PersonHoursPerDay, 0))
							             						  WHEN f.Date<=	EOMONTH(@ActualsEndDateLocal) THEN (f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount -
																																		(CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate 
																																			  THEN f.SLHR  ELSE f.PayRate+f.MLFOverheadRate END) 
																																		*ISNULL(f.PersonHoursPerDay, 0)) ELSE 0 END )  ELSE 0 END)
								 ,0) BudgetMargin,
				   ISNULL(SUM(CASE WHEN @ActualsEndDateLocal IS NULL THEN f.PersonHoursPerDay
								   WHEN f.Date<=@ActualsEndDateLocal THEN f.PersonHoursPerDay ELSE 0 END), 0) AS BudgetHours
			INTO #Budget
			FROM #BudgetRestrospective AS f
			WHERE f.ProjectId=@ProjectIdLocal AND f.PersonId IS NOT NULL  AND f.Date >= @startDateLocal AND f.Date <= @endDateLocal
			GROUP BY f.ProjectId, f.PersonId 


	  --Budget Resource Data
	  SELECT B.ProjectId,
			 B.PersonId,
			 Per.FirstName,
			 Per.LastName,
			 CONVERT(DECIMAL(6,2),B.BudgetHours) BudgetHours,
			 CONVERT(DECIMAL(18,2),B.BudgetMargin) as BudgetMargin,
			 CONVERT(DECIMAL(18,2),B.Revenue) as BudgetRevenue
	  FROM #Budget B
	  INNER JOIN v_Person Per on B.PersonId=Per.PersonId

	  SELECT A.ProjectId,
			 A.PersonId,
			 Per.FirstName,
			 Per.LastName,
			 CONVERT(DECIMAL(6,2), ISNULL(A.ActualHours,0)) as ActualHours,
			 CONVERT(DECIMAL(18,2), ISNULL(A.ActualMargin,0)) as ActualMargin,
			 CONVERT(DECIMAL(18,2), ISNULL(A.ActualRevenue,0)) as ActualRevenue,
			 CONVERT(DECIMAL(6,2), ISNULL(A.ProjectedHours,0)) as ProjectedHours,
			 CONVERT(DECIMAL(18,2), ISNULL(A.ProjectedRevenue,0)) as ProjectedRevenue,
			 CONVERT(DECIMAL(18,2), ISNULL(A.ProjectedMargin,0)) as ProjectedMargin,
			 CONVERT(DECIMAL(6,2), ISNULL(A.RemainingProjectedHours,0)) as RemainingProjectedHours,
			 CONVERT(DECIMAL(18,2), ISNULL(A.RemainingProjectedRevenue,0)) as RemainingProjectedRevenue,
			 CONVERT(DECIMAL(18,2), ISNULL(A.RemainingProjectedMargin,0)) as RemainingProjectedMargin,
			 CONVERT(DECIMAL(6,2), ISNULL(A.EACHours,0)) as EACHours,
			 CONVERT(DECIMAL(18,2), ISNULL(A.EACRevenue,0)) as EACRevenue,
			 CONVERT(DECIMAL(18,2), ISNULL(A.EACMargin,0)) as EACMargin
	  FROM  #ActualsAndProjected A 
	  INNER JOIN v_Person Per on A.PersonId=Per.PersonId
	  ORDER BY Per.LastName
	  --------------

	  SELECT PDE.ProjectId,
			 PDE.ExpenseId,
			 PDE.Name,
		     SUM(CASE WHEN (@ActualsEndDateLocal IS NULL OR PDE.date <= @ActualsEndDateLocal )THEN PDE.EstimatedAmount ELSE 0 END) as ProjectedAmount,
		     SUM(CASE WHEN (@ActualsEndDateLocal IS NOT NULL AND PDE.date > @ActualsEndDateLocal )THEN PDE.EstimatedAmount ELSE 0 END) as EstimatedAmount,
		     SUM(CASE WHEN (@ActualsEndDateLocal IS NULL OR PDE.date <= @ActualsEndDateLocal )THEN PDE.ActualExpense ELSE 0 END ) as ActualAmount
	  INTO #ProjectExpenses
	  FROM v_ProjectDailyExpenses PDE
	  WHERE PDE.ProjectId = @ProjectIdLocal and PDE.date>=@startDateLocal and PDE.date<=@endDateLocal
	  GROUP BY PDE.ProjectId, PDE.ExpenseId, PDE.Name

	  SELECT VB.ProjectId,
			 VB.ExpenseId,
			 VB.Name,
			 SUM(CASE WHEN (@ActualsEndDateLocal IS NULL OR VB.date <= @ActualsEndDateLocal )THEN ISNULL(VB.Amount,0) ELSE 0 END) as Amount
	  INTO #ProjectBudgetExpenses
	  FROM v_ProjectBudgetDailyExpenses VB
	  WHERE VB.ProjectId = @ProjectIdLocal AND VB.date>=@startDateLocal and VB.date<=@endDateLocal
	  GROUP BY VB.ProjectId, VB.ExpenseId, VB.Name

	  SELECT ISNULL(PBE.ExpenseId,PE.ExpenseId) as Id,
			 ISNULL(PBE.Name, PE.Name) as Name,
			 ISNULL(PBE.Amount,0) as BudgetExpense,
			 ISNULL(PE.ProjectedAmount,0) as ProjectedExpense,
			 ISNULL(PE.ActualAmount,0) as ActualExpense,
			 ISNULL(PE.EstimatedAmount,0) as ProjectedRemainingExpense
	  FROM #ProjectBudgetExpenses PBE
	  FULL JOIN #ProjectExpenses PE ON PE.ExpenseId=PBE.ExpenseId
	  -----------

	  SELECT P.ProjectId,
			 P.Budget,
			 CASE WHEN p.ExceptionMargin IS NOT NULL THEN P.ExceptionMargin
				  WHEN C.MarginGoal IS NOT NULL THEN C.MarginGoal
				  ELSE 50
				  END AS MarginGoal,
			dbo.GetNumberOfCompanyWorkingDays(p.StartDate, p.EndDate) as TotalProjectDays,
			dbo.GetNumberOfCompanyWorkingDays(@Today, P.EndDate) as RemainingDays
	  FROM Project P
	  FULL JOIN ClientMarginGoal C on P.ClientId=c.ClientId and ((c.StartDate is null or  c.StartDate<=p.StartDate) and (c.EndDate is null or c.EndDate>=p.StartDate))
	  WHERE p.ProjectId=@ProjectIdLocal 

	  DROP Table #Budget
	  DROP TABLE #ActualsAndProjected
	  DROP TABLE #ProjectExpenses
	  DROP TABLE #ProjectBudgetExpenses
	  DROP TABLE #BudgetRestrospective
END 
