﻿CREATE PROCEDURE dbo.FinancialsListByProjectPeriodTotal
(
	@ProjectId   NVARCHAR(MAX),
	@StartDate   DATETIME = NULL,
	@EndDate     DATETIME  = NULL,
	@UseActuals  BIT = 0
)  WITH RECOMPILE
AS
BEGIN

	SET NOCOUNT ON
	SET ANSI_WARNINGS OFF


	DECLARE @ProjectIdLocal   NVARCHAR(MAX),
			@StartDateLocal   DATETIME,
			@EndDateLocal     DATETIME,
			@UseActualsLocal  BIT,
			@FutureDate DATETIME,
			@HoursPerYear INT

	SELECT @ProjectIdLocal =@ProjectId ,
		   @StartDateLocal=@StartDate ,
		   @EndDateLocal=@EndDate,
		   @UseActualsLocal = @UseActuals,
		   @FutureDate = dbo.GetFutureDate(),
		   @HoursPerYear = dbo.BonusHoursPerYearFunction()
		     
	DECLARE @ProjectIds TABLE
	(
		ProjectId	INT PRIMARY KEY
	)
	INSERT INTO @ProjectIds
	SELECT ResultId FROM dbo.ConvertStringListIntoTable(@ProjectIdLocal)
	
	DECLARE @Today DATETIME, @CurrentMonthStartDate DATETIME,@InsertingTime DATETIME

	SELECT @Today = CONVERT(DATE, dbo.GettingPMTime(GETUTCDATE())),@InsertingTime = dbo.InsertingTime()
	SELECT @CurrentMonthStartDate = C.MonthStartDate
	FROM dbo.Calendar C
	WHERE C.Date = @Today

	IF @StartDateLocal IS NULL OR @EndDateLocal IS  NULL
	BEGIN

		SELECT @StartDateLocal	=	MIN(P2.StartDate),
			   @EndDateLocal	=	MAX(P2.EndDate)
		FROM @ProjectIds P1
		JOIN Project P2 ON P1.ProjectId = P2.ProjectId

	END
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
				WHERE Pro.IsAllowedToShow = 1 AND Pro.ProjectId IN (SELECT ProjectId FROM @ProjectIds)
				GROUP BY Pro.ProjectId
			 ) P ON p.ProjectId = CC.ProjectId
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
			-- dbo.PersonProjectedHoursPerDay(cal.DayOff,cal.companydayoff,cal.TimeoffHours,mpe.HoursPerDay) AS HoursPerDay,
			--Removed Inline Function for the sake of performance. When ever PersonProjectedHoursPerDay function is updated need to update below case when also.
			SUM(CONVERT(DECIMAL(4,2),CASE WHEN cal.DayOff = 0  THEN mpe.HoursPerDay -- No Time-off and no company holiday
				WHEN (cal.DayOff = 1 and cal.companydayoff = 1) OR (cal.DayOff = 1 AND cal.companydayoff = 0 AND ISNULL(cal.TimeoffHours,8) = 8) THEN 0 -- only company holiday OR person complete dayoff
				ELSE mpe.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off
			END)) AS HoursPerDay,
			SUM(CONVERT(DECIMAL(4,2),CASE WHEN cal.DayOff = 0  THEN mpe.HoursPerDay -- No Time-off and no company holiday
				WHEN (cal.DayOff = 1 and cal.companydayoff = 1) OR (cal.DayOff = 1 AND cal.companydayoff = 0 AND ISNULL(cal.TimeoffHours,8) = 8) THEN 0 -- only company holiday OR person complete dayoff
				ELSE mpe.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off
			END) * mpe.Amount) AS PersonMilestoneDailyAmount--PersonLevel
			FROM dbo.Project P 
			INNER JOIN dbo.[Milestone] AS m ON P.ProjectId=m.ProjectId AND p.IsAllowedToShow = 1 AND p.projectid != 174
			INNER JOIN dbo.MilestonePerson AS mp ON m.[MilestoneId] = mp.[MilestoneId]
			INNER JOIN dbo.MilestonePersonEntry AS mpe ON mp.MilestonePersonId = mpe.MilestonePersonId
			INNER JOIN dbo.PersonCalendarAuto AS cal ON cal.Date BETWEEN mpe.Startdate AND mpe.EndDate AND cal.PersonId = mp.PersonId 
			WHERE P.ProjectId IN (SELECT ProjectId FROM @ProjectIds) 
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
	MilestoneRevenueRetrospective
	AS
	(
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
	ctePersonPayRetrospective
	AS
	(
	SELECT cal.PersonId,
	       cal.Date,
	       p.Timescale,
	       CASE
	           WHEN p.Timescale IN (1, 3, 4)
	           THEN p.Amount
	           ELSE p.Amount / HY.HoursInYear
	       END AS HourlyRate,
  		   p.BonusAmount / (CASE WHEN p.BonusHoursToCollect = @HoursPerYear THEN HY.HoursInYear ELSE NULLIF(p.BonusHoursToCollect,0) END) AS BonusRate,
		   p.VacationDays
		   FROM dbo.PersonCalendarAuto AS cal
		   INNER JOIN dbo.Pay AS p ON cal.PersonId = p.Person AND p.StartDate <= cal.Date AND p.EndDate > cal.date  
	       INNER JOIN V_WorkinHoursByYear HY ON cal.Date BETWEEN HY.[YearStartDate] AND HY.[YearEndDate]
	),
	cteFinancialsRetrospectiveActualHours
	as
	(
	SELECT	ISNULL(ME.ProjectId,AE.ProjectId) AS ProjectId,
		ISNULL(ME.PersonId,AE.PersonId) AS PersonId,
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
		INNER JOIN dbo.Calendar C ON c.Date = ISNULL(ME.Date,AE.ChargeCodeDate)
		LEFT JOIN ctePersonPayRetrospective AS p ON p.PersonId = ISNULL(ME.PersonId,AE.PersonId) AND p.Date = c.Date
		LEFT JOIN v_MLFOverheadFixedRateTimescale MLFO ON MLFO.TimescaleId = p.Timescale AND c.Date BETWEEN MLFO.StartDate AND ISNULL(MLFO.EndDate,@FutureDate)
		LEFT JOIN dbo.v_OverheadFixedRateTimescale AS o ON p.Date BETWEEN o.StartDate AND ISNULL(o.EndDate, @FutureDate) AND o.TimescaleId = p.Timescale
		LEFT JOIN MilestoneRevenueRetrospective AS r ON ME.MilestoneId = r.MilestoneId AND c.Date = r.Date
		GROUP BY ISNULL(ME.ProjectId,AE.ProjectId),ISNULL(ME.PersonId,AE.PersonId),c.Date,C.DaysInYear,AE.BillableHOursPerDay,AE.NonBillableHoursPerDay,ME.IsHourlyAmount,ME.HoursPerDay,ME.PersonMilestoneDailyAmount,
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
		   f.BillRate,
		   f.OverheadRate,
		   f.BonusRate,
		   f.VacationRate
	FROM cteFinancialsRetrospectiveActualHours f
	WHERE f.Date BETWEEN @StartDateLocal AND @EndDateLocal
	),
	ActualAndProjectedValuesDaily
	AS
	(
	SELECT	f.ProjectId,
			f.PersonId,
			f.Date,
			SUM(f.PersonMilestoneDailyAmount) AS ProjectedRevenueperDay,
			SUM(f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount) AS ProjectedRevenueNet,
			SUM(f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount - (
																				CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate THEN f.SLHR 
																				ELSE f.PayRate + f.MLFOverheadRate END
																			) * ISNULL(f.PersonHoursPerDay, 0)) AS  ProjectedGrossMargin,
			ISNULL(SUM((CASE WHEN f.SLHR >=  f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END)*ISNULL(f.PersonHoursPerDay, 0)),0) ProjectedCogsperDay,
			SUM(CASE WHEN f.IsHourlyAmount = 0 THEN f.PersonMilestoneDailyAmount ELSE 0 END ) AS FixedActualRevenuePerDay,
			(ISNULL(SUM(f.BillRate* f.ActualHoursPerDay),0) / ISNULL(NULLIF(SUM(f.ActualHoursPerDay),0),1)) * MAX(CASE WHEN f.IsHourlyAmount = 1 THEN  f.BillableHOursPerDay ELSE 0 END) HourlyActualRevenuePerDay,
			SUM(CASE WHEN f.IsHourlyAmount = 0 
					THEN 	f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount - (CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate  THEN f.SLHR ELSE f.PayRate + f.MLFOverheadRate END) * ISNULL(f.PersonHoursPerDay, 0)
					ELSE 0
				END) AS FixedActualMarginPerDay,
				((ISNULL(SUM(f.BillRate* f.ActualHoursPerDay),0) / ISNULL(NULLIF(SUM(f.ActualHoursPerDay),0),1)) * MAX(CASE WHEN f.IsHourlyAmount = 1 THEN  f.BillableHOursPerDay ELSE 0 END))
				 -  (
						MAX( CASE WHEN f.IsHourlyAmount = 1 THEN  f.BillableHOursPerDay + f.NonBillableHoursPerDay ELSE 0 END ) * 
						ISNULL( CASE WHEN ISNULL(SUM(f.ActualHoursPerDay),0) > 0 
									THEN SUM((CASE WHEN f.SLHR >=  f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END)* f.ActualHoursPerDay) / SUM(f.ActualHoursPerDay)
									ELSE SUM((CASE WHEN f.SLHR >=  f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END)) / ISNULL(COUNT(f.PayRate),1)  
								END ,0)
					) AS HourlyActualMarginPerDay,
			ISNULL(SUM(f.PersonHoursPerDay), 0) AS ProjectedHoursPerDay,
		  	min(f.Discount) as Discount,
			MIN(CONVERT(INT, f.IsHourlyAmount)) as IsHourlyAmount
	FROM FinancialsRetro AS f
	GROUP BY f.ProjectId, f.PersonId, f.Date
	),
	ProjectExpenses
	AS
	(
		SELECT pexp.ProjectId,
			CONVERT(DECIMAL(18,2),SUM(ISNULL(pexp.Amount,0)/((DATEDIFF(dd,pexp.StartDate,pexp.EndDate)+1)))) Expense,
			CONVERT(DECIMAL(18,2),SUM(pexp.Reimbursement*0.01*ISNULL(pexp.Amount,0)/((DATEDIFF(dd,pexp.StartDate,pexp.EndDate)+1)))) Reimbursement
		FROM dbo.ProjectExpense as pexp
		JOIN dbo.Calendar c ON c.Date BETWEEN pexp.StartDate AND pexp.EndDate
		WHERE ProjectId IN (SELECT ProjectId FROM @ProjectIds) AND c.Date BETWEEN @StartDateLocal AND @EndDateLocal
		GROUP BY pexp.ProjectId
	), 
	ActualAndProjectedValues  AS
	(SELECT CT.ProjectId, 
			SUM(ISNULL(CT.ProjectedRevenueperDay, 0)) AS ProjectedRevenue,
			SUM(ISNULL(CT.ProjectedRevenueNet, 0)) as ProjectedRevenueNet,
			SUM(ISNULL(CT.ProjectedGrossMargin, 0)) as ProjectedGrossMargin,
			SUM(ISNULL(CT.HourlyActualRevenuePerDay, 0) + ISNULL(CT.FixedActualRevenuePerDay, 0)) AS ActualRevenue,
			SUM(ISNULL(CT.HourlyActualMarginPerDay, 0) + ISNULL(CT.FixedActualMarginPerDay, 0)) as ActualMargin,
			SUM(ISNULL(CT.ProjectedCogsperDay, 0)) as ProjectedCogs,
			MAX(ISNULL(CT.Discount, 0)) Discount,
			SUM(ISNULL(CT.ProjectedHoursPerDay, 0)) as ProjectedHoursPerMonth,
			SUM(CASE WHEN CT.Date < @CurrentMonthStartDate THEN ISNULL(CT.HourlyActualRevenuePerDay, 0) + ISNULL(CT.FixedActualRevenuePerDay, 0)
					 ELSE ISNULL(CT.ProjectedRevenueperDay, 0) END) AS PreviousMonthActualRevenue,
			SUM(CASE WHEN CT.Date < @CurrentMonthStartDate THEN CONVERT(DECIMAL(18,6),ISNULL(CT.HourlyActualMarginPerDay, 0) + ISNULL(CT.FixedActualMarginPerDay, 0))
					 ELSE ISNULL(CT.ProjectedGrossMargin, 0) END) AS PreviousMonthActualGrossMargin
	FROM ActualAndProjectedValuesDaily CT
	INNER JOIN dbo.Calendar C ON C.Date = CT.Date 
	GROUP BY CT.ProjectId
	)
	SELECT
		APV.ProjectId,
		P.StartDate FinancialDate,
		P.EndDate MonthEnd,
		'Total' AS RangeType,
		CONVERT(DECIMAL(18,2),ISNULL(APV.ProjectedRevenue,0)) AS 'Revenue',
		/*Revenue(total) = Sum of all persons Daily amout  for the days they are assigned  */
		CONVERT(DECIMAL(18,2),ISNULL(APV.ProjectedRevenueNet,0) + ISNULL(PEM.Reimbursement,0))   as 'RevenueNet',
		/*RevenueNet =  Revenue(total) - ProjectDiscount */
		CONVERT(DECIMAL(18,2),ISNULL(APV.ProjectedCogs,0)) Cogs ,
		CONVERT(DECIMAL(18,2),ISNULL(APV.ProjectedGrossMargin,0)+(ISNULL(PEM.Reimbursement,0)-ISNULL(PEM.Expense,0)))  as 'GrossMargin',
		/*Gross Margin =  RevenueNet - Cogs - expenses + Reimbursement expenses */
		CONVERT(DECIMAL(18,2),ISNULL(APV.ProjectedHoursPerMonth,0)) Hours,
		CONVERT(DECIMAL(18,2),ISNULL(PEM.Expense,0)) Expense,
		CONVERT(DECIMAL(18,2),ISNULL(PEM.Reimbursement,0)) ReimbursedExpense,
		CONVERT(DECIMAL(18,6), ISNULL(APV.ActualRevenue,0)) ActualRevenue,
		CONVERT(DECIMAL(18,6), (ISNULL(APV.ActualMargin,0)) - (ISNULL(APV.ActualRevenue,0) * (ISNULL(APV.Discount,0)/100)) + ((ISNULL(PEM.Reimbursement,0)-ISNULL(PEM.Expense,0))))  ActualGrossMargin,
		CONVERT(DECIMAL(18,6), ISNULL(APV.PreviousMonthActualRevenue,0)+(ISNULL(PEM.Reimbursement,0)-ISNULL(PEM.Expense,0))) AS PreviousMonthActualRevenue,
		CONVERT(DECIMAL(18,6), ISNULL(APV.PreviousMonthActualGrossMargin,0)+(ISNULL(PEM.Reimbursement,0)-ISNULL(PEM.Expense,0))- (ISNULL(APV.PreviousMonthActualGrossMargin,0) * (ISNULL(APV.Discount,0)/100))) AS PreviousMonthActualGrossMargin,
		CONVERT(BIT,0) AS IsMonthlyRecord,
		@InsertingTime AS  CreatedDate,	
		CONVERT(DATE,@InsertingTime)  AS CacheDate
	FROM ActualAndProjectedValues APV
	JOIN Project p on (p.ProjectId = APV.ProjectId)
	LEFT JOIN ProjectExpenses PEM ON PEM.ProjectId = APV.ProjectId 

END

