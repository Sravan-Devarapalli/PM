CREATE PROCEDURE [dbo].[ProjectBudgetComaparisonSummary]
	(
	  @ProjectNumber NVARCHAR(12) ,
	  @StartDate DATETIME = NULL ,
	  @EndDate DATETIME = NULL,
	  @ActualsEndDate DATETIME = NULL
	)
AS 
BEGIN
	SET NOCOUNT ON;
		DECLARE @StartDateLocal DATETIME = NULL ,
				@EndDateLocal DATETIME = NULL ,
				@ProjectNumberLocal NVARCHAR(12) ,
				@ProjectId INT = NULL ,
				@Today DATE,
				@FutureDate DATETIME,
				@CurrentMonthEnd DATETIME,
				@LastMonthEnd DATETIME,
				@ProjRemainingDate DATETIME

		SELECT @ProjectNumberLocal = @ProjectNumber,@FutureDate = dbo.GetFutureDate()
		SELECT @Today = CONVERT(DATE, dbo.GettingPMTime(GETUTCDATE()))
		SELECT @CurrentMonthEnd =EOMONTH ( @Today )

		SELECT @LastMonthEnd=convert (date,DATEADD(MONTH, DATEDIFF(MONTH, -1, @Today)-1, -1))
		SELECT @ProjRemainingDate = CASE WHEN @ActualsEndDate IS NULL THEN @Today ELSE @ActualsEndDate END
	
		SELECT  @ProjectId = P.ProjectId
		FROM    dbo.Project AS P
		WHERE   P.ProjectNumber = @ProjectNumberLocal
				AND @ProjectNumberLocal != 'P999918' and P.IsInternal =0--Business Development Project 


		IF ( @ProjectId IS NOT NULL ) 
			BEGIN

				SET @Today = dbo.GettingPMTime(GETUTCDATE())
				

				IF ( @StartDate IS NOT NULL
					 OR @EndDate IS NOT NULL
				   ) 
					BEGIN
						SET @StartDateLocal = CONVERT(DATE, @StartDate)
						SET @EndDateLocal = CONVERT(DATE, @EndDate)
					END
				ELSE
					BEGIN
						 SELECT @startDateLocal= p.StartDate, @endDateLocal=p.EndDate
						 FROM Project P
						 WHERE P.ProjectId=@ProjectId
					END

			
		;WITH ActualTimeEntries
		AS 
		(
			SELECT  CC.ProjectId,
					TE.PersonId,
					TE.ChargeCodeDate,
					SUM(CASE WHEN TEH.IsChargeable = 1 THEN TEH.ActualHours ELSE 0 END) BillableHOursPerDay,
					SUM(CASE WHEN TEH.IsChargeable = 0 THEN TEH.ActualHours ELSE 0 END) NonBillableHoursPerDay,
					P.IsHourlyAmount
			FROM TimeEntry TE
			JOIN TimeEntryHours TEH ON TEH.TimeEntryId = TE.TimeEntryId
			JOIN ChargeCode CC on CC.Id = TE.ChargeCodeId 
			JOIN (
					SELECT Pro.ProjectId,CAST(CASE WHEN SUM(CAST(m.IsHourlyAmount as INT)) > 0 THEN 1 ELSE 0 END AS BIT) AS IsHourlyAmount
					FROM Project Pro 
						LEFT JOIN Milestone m ON m.ProjectId = Pro.ProjectId 
					WHERE Pro.IsAllowedToShow = 1 AND Pro.ProjectId =@ProjectId
					GROUP BY Pro.ProjectId
				 ) P ON p.ProjectId = CC.ProjectId
			WHERE (TE.ChargeCodeDate>=@startDateLocal AND TE.ChargeCodeDate<=@endDateLocal) AND (@ActualsEndDate IS NULL OR TE.ChargeCodeDate<=@ActualsEndDate)
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
						WHEN ( cal.companydayoff = 1) OR ( cal.companydayoff = 0 AND ISNULL(cal.TimeoffHours,8) = 8) THEN 0 -- only company holiday OR person complete dayoff
						ELSE mpe.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off
					END)) AS HoursPerDay,
					SUM(CONVERT(DECIMAL(4,2),CASE WHEN cal.DayOff = 0  THEN mpe.HoursPerDay -- No Time-off and no company holiday
						WHEN ( cal.companydayoff = 1) OR ( cal.companydayoff = 0 AND ISNULL(cal.TimeoffHours,8) = 8) THEN 0 -- only company holiday OR person complete dayoff
						ELSE mpe.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off
					END) * mpe.Amount) AS PersonMilestoneDailyAmount--PersonLevel
					FROM dbo.Project P 
					INNER JOIN dbo.[Milestone] AS m ON P.ProjectId=m.ProjectId AND p.IsAllowedToShow = 1 AND p.projectid != 174
					INNER JOIN dbo.MilestonePerson AS mp ON m.[MilestoneId] = mp.[MilestoneId]
					INNER JOIN dbo.MilestonePersonEntry AS mpe ON mp.MilestonePersonId = mpe.MilestonePersonId
					INNER JOIN dbo.PersonCalendarAuto AS cal ON cal.Date BETWEEN mpe.Startdate AND mpe.EndDate AND cal.PersonId = mp.PersonId 
					WHERE P.ProjectId =@ProjectId AND cal.Date BETWEEN mpe.Startdate AND mpe.EndDate AND cal.PersonId = mp.PersonId 
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
			 SELECT --Milestones with a fixed amount and monthly revenues
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
					ISNULL((m.Amount/ NULLIF(MTHours.TotalHours,0))* d.HoursPerDay,0) AS MilestoneDailyAmount ,
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

		    FinancialsRetro 
			AS 
			(
			SELECT f.ProjectId,
				   f.Date, 
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
				   f.PersonMilestoneDailyAmount,
		           f.PersonDiscountDailyAmount
			FROM cteFinancialsRetrospectiveActualHours f
			WHERE f.Date BETWEEN @startDateLocal AND @endDateLocal and f.ProjectId=@ProjectId
			)

			SELECT	f.ProjectId,
					f.Date,
					f.PersonId,
					CONVERT(DECIMAL(18,2),f.BillRate) BillRate,
					SUM(f.PersonMilestoneDailyAmount) AS ProjectedRevenueperDay,
					SUM(f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount - (
																					CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate THEN f.SLHR 
																					ELSE f.PayRate + f.MLFOverheadRate END
																				) * ISNULL(f.PersonHoursPerDay, 0)) AS  ProjectedGrossMargin,
				    SUM(CASE WHEN f.IsHourlyAmount = 0 THEN f.PersonMilestoneDailyAmount ELSE 0 END ) AS FixedActualRevenuePerDay,
					(ISNULL(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.BillRate* f.ActualHoursPerDay ELSE 0 END),0) / ISNULL(NULLIF(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.ActualHoursPerDay ELSE 0 END),0),1)) * MAX(CASE WHEN f.IsHourlyAmount = 1 THEN  f.BillableHOursPerDay ELSE 0 END) HourlyActualRevenuePerDay,
					--SUM(CASE WHEN (f.IsHourlyAmount = 0 and (@ActualsEndDate IS NULL OR f.Date<=@ActualsEndDate)) THEN 	f.PersonMilestoneDailyAmount ELSE 0 END)- (
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
					CONVERT(DECIMAL(18,2),ISNULL(SUM(CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END),0)) as PersonCost,
					ISNULL(SUM(f.PersonHoursPerDay), 0) AS ProjectedHoursPerDay,
					SUM(f.BillableHOursPerDay + f.NonBillableHoursPerDay) as ActualHoursPerDay,
					ISNULL(SUM(CASE WHEN (f.Date > @Today AND f.BillableHOursPerDay IS NULL AND f.NonBillableHoursPerDay IS NULL) THEN f.PersonHoursPerDay ELSE 0 END),0) as RemainingProjectedHoursPerDay,
					SUM(CASE WHEN f.IsHourlyAmount = 0 AND f.Date > EOMONTH(@ProjRemainingDate)THEN f.PersonMilestoneDailyAmount 
							 WHEN f.IsHourlyAmount = 1 AND (f.Date > @ProjRemainingDate AND f.BillableHOursPerDay IS NULL AND f.NonBillableHoursPerDay IS NULL) THEN f.PersonMilestoneDailyAmount
							 ELSE 0 END )as  RemRevenue,
					SUM(CASE WHEN f.IsHourlyAmount = 0 AND f.Date > @ProjRemainingDate THEN (CASE WHEN f.Date > EOMONTH(@ProjRemainingDate) THEN (f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount) ELSE 0 END - (
																					CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate THEN f.SLHR 
																					ELSE f.PayRate + f.MLFOverheadRate END
																				) * ISNULL(f.PersonHoursPerDay, 0)) 
				  WHEN f.IsHourlyAmount = 1 AND (f.Date > @ProjRemainingDate AND f.BillableHOursPerDay IS NULL AND f.NonBillableHoursPerDay IS NULL) THEN f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount -
						(CASE WHEN f.SLHR  >= f.PayRate + f.MLFOverheadRate 
							  THEN f.SLHR ELSE f.PayRate + f.MLFOverheadRate END) 
					    *ISNULL(f.PersonHoursPerDay, 0)
						 ELSE 0 END) as RemGrossMargin
			INTO #ActualAndProjectedValuesDaily
			FROM FinancialsRetro AS f
			GROUP BY f.ProjectId,f.PersonId, f.Date, f.BillRate
			

			
			SELECT A.ProjectId,
				   A.PersonId,
				   C.MonthStartDate AS FinancialDate, 
				   CONVERT(DECIMAL(6,2),SUM(ISNULL(A.ProjectedHoursPerDay,0))) as ProjectedHours,
				    CONVERT(DECIMAL(6,2),SUM(ISNULL(A.ActualHoursPerDay,0))) as ActualHours,
				    CONVERT(DECIMAL(6,2),SUM(ISNULL(A.RemainingProjectedHoursPerDay,0))) as RemainingProjectedHours
			INTO #ActualAndProjectedMonthly
			FROM #ActualAndProjectedValuesDaily A
			JOIN dbo.Calendar C on c.Date=A.Date
			GROUP BY A.ProjectId, A.PersonId, C.MonthStartDate, C.MonthEndDate, C.MonthNumber

			SELECT CT.ProjectId,
				   CT.PersonId,
				   SUM(ISNULL(CT.ProjectedRevenueperDay,0)) as ProjectedRevenue,
				   SUM(ISNULL(CT.ProjectedGrossMargin,0)) as ProjectedMargin, 
				   SUM(ISNULL(CT.HourlyActualRevenuePerDay, 0) + CASE WHEN CT.Date<= EOMONTH(@ProjRemainingDate) THEN ISNULL(CT.FixedActualRevenuePerDay, 0) ELSE 0 END) AS ActualRevenue,
				   SUM(ISNULL(CT.HourlyActualRevenuePerDay, 0)- ISNULL(CT.HourlyActualCostPerDay,0) +  CASE WHEN  CT.Date<= EOMONTH(@ProjRemainingDate) THEN ISNULL(CT.FixedActualRevenuePerDay, 0) ELSE 0 END  - ISNULL(CT.FixedActualCostPerDay,0) ) as ActualMargin,
			       SUM(ISNULL(CT.RemRevenue, 0)+ISNULL(CT.HourlyActualRevenuePerDay, 0) + CASE WHEN CT.Date<=EOMONTH(@ProjRemainingDate) THEN ISNULL(CT.FixedActualRevenuePerDay, 0) ELSE 0 END) AS ETCRevenue,
			       SUM(ISNULL(CT.RemGrossMargin,0) + ISNULL(CT.HourlyActualRevenuePerDay, 0)- ISNULL(CT.HourlyActualCostPerDay,0) +  CASE WHEN CT.Date<=EOMONTH(@ProjRemainingDate) THEN ISNULL(CT.FixedActualRevenuePerDay, 0) ELSE 0 END - ISNULL(CT.FixedActualCostPerDay,0)) as ETCMargin
		   INTO #ActualAndProjectedRevenue
		   FROM #ActualAndProjectedValuesDaily CT
		   GROUP BY CT.ProjectId, CT.PersonId

			SELECT A.PersonId,
				   MIN(A.Date) as StartDate,
				   A.BillRate,
				   A.PersonCost as PayRate
			INTO #PersonBillRatePeriods
			FROM #ActualAndProjectedValuesDaily A
			WHERE A.BillRate IS NOT NULL AND A.BillRate!=0
			GROUP BY A.PersonId, A.BillRate, A.PersonCost
			
			
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
		EXEC [dbo].[FinancialsRetrospectiveBudget] @ProjectId = @projectid

		
			SELECT f.ProjectId,
				   f.Date,
				   f.PersonId,
				   CONVERT(DECIMAL(18,2),f.BillRate) as BillRate,
				   CONVERT(DECIMAL(18,2),ISNULL(SUM(CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END),0)) as PersonCost,
				  SUM(ISNULL(f.PersonHoursPerDay,0)) as BudgetHoursDaily,
				  ISNULL(SUM(f.PersonMilestoneDailyAmount),0) AS BudgetRevenuePerDay,
				  ISNULL(SUM(f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount -
								(CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate 
									  THEN f.SLHR  ELSE f.PayRate+f.MLFOverheadRate END) 
								*ISNULL(f.PersonHoursPerDay, 0)),0) BudgetMarginPerDay
			 INTO #BudgetValuesDaily
 			  FROM #BudgetRestrospective AS f
			  WHERE f.Date BETWEEN @StartDateLocal AND @EndDateLocal and f.projectid=@projectid
			  GROUP BY f.ProjectId, f.PersonId, f.Date, f.BillRate
			
		
			SELECT B.ProjectId,
				   B.PersonId,
				   C.MonthStartDate AS FinancialDate, 
				   C.MonthEndDate AS MonthEnd,
				   C.MonthNumber,
				    CONVERT(DECIMAL(6,2),SUM(ISNULL(B.BudgetHoursDaily,0))) as BudgetHours
			INTO #BudgetValuesMonthly
			FROM #BudgetValuesDaily AS B
			JOIN dbo.Calendar C on c.Date=B.Date
			GROUP BY B.ProjectId, B.PersonId, C.MonthStartDate, C.MonthEndDate, C.MonthNumber

			SELECT B.ProjectId,
				   B.PersonId,
				    SUM(ISNULL(B.BudgetRevenuePerDay,0)) as BudgetRevenue,
				   SUM(ISNULL(B.BudgetMarginPerDay,0)) as BudgetMargin
			INTO #BudgetRevenue
			FROM #BudgetValuesDaily B
			GROUP BY B.ProjectId, B.PersonId

			SELECT B.PersonId,
				   MIN(B.Date) as StartDate,
				   B.BillRate,
				   B.PersonCost as PayRate
			INTO #BudgetBillRatePeriod
			FROM #BudgetValuesDaily B
			WHERE B.BillRate IS NOT NULL AND B.BillRate!=0
			GROUP BY B.PersonId, B.BillRate, B.PersonCost

			-- select actuals and projected Resource Data
		    select A.PersonId,
				   P.FirstName,
				   P.LastName,
				   T.TitleId,
				   T.Title,
				   A.FinancialDate,
				   A.ProjectedHours,
				   A.ActualHours,
				   A.RemainingProjectedHours 
			from #ActualAndProjectedMonthly A
			JOIN dbo.Person P on A.PersonId = P.PersonId
			LEFT JOIN dbo.Title AS T ON p.TitleId = T.TitleId 
			ORDER BY P.LastName, A.FinancialDate

			--select actuals and projected Resource Bill rate Data
			SELECT * FROM #PersonBillRatePeriods
			ORDER BY StartDate

			--select actuals and projected Resource Revenue Data

			SELECT A.PersonId,
				   CONVERT(DECIMAL(18,2),A.ActualRevenue) as ActualRevenue,
				   CONVERT(DECIMAL(18,2),A.ActualMargin) as ActualMargin,
				   CONVERT(DECIMAL(18,2),A.ProjectedRevenue) as ProjectedRevenue,
				   CONVERT(DECIMAL(18,2),A.ProjectedMargin) as ProjectedMargin,
				   CONVERT(DECIMAL(18,2),A.ETCRevenue) as EACRevenue,
				   CONVERT(DECIMAL(18,2),A.ETCMargin) as EACMargin
			FROM #ActualAndProjectedRevenue A


			--select Budget Resource Data
			select B.PersonId,
				   P.FirstName,
				   P.LastName,
				   T.TitleId,
				   T.Title,
				   B.FinancialDate,
				   B.BudgetHours
			from #BudgetValuesMonthly B
			JOIN dbo.Person P on B.PersonId = P.PersonId
			LEFT JOIN dbo.Title AS T ON p.TitleId = T.TitleId 
			ORDER BY P.LastName, B.FinancialDate

			--select Budget Bill rate Data
			SELECT * FROM #BudgetBillRatePeriod
			ORDER BY StartDate

			--select Budget resource revenues

			SELECT B.PersonId,
				   CONVERT(DECIMAL(18,2),B.BudgetRevenue) as BudgetRevenue,
				   CONVERT(DECIMAL(18,2),B.BudgetMargin) as BudgetMargin
			FROM #BudgetRevenue B

		DROP TABLE #BudgetRestrospective
        DROP TABLE #ActualAndProjectedValuesDaily
        DROP TABLE #ActualAndProjectedMonthly
        DROP TABLE #PersonBillRatePeriods
        DROP TABLE #BudgetValuesMonthly
        DROP TABLE #BudgetBillRatePeriod
        DROP TABLE #BudgetValuesDaily
		DROP TABLE #BudgetRevenue
		
		
			END
		ELSE 
			BEGIN
				RAISERROR('There is no Project with this Project Number.', 16, 1)
			END
END

