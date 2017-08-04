CREATE PROCEDURE [dbo].[BudgetManagementByProject]
(
	@ProjectId   INT,
	@BudgetToDate BIT = 0,
	@ActualsEndDate DATETIME = NULL
)
AS
BEGIN
	DECLARE @ProjectIdLocal	INT,
			@Today	DATETIME,
			@CurrentMonthEnd DATETIME,
			@LastMonthEnd DATETIME

	SET @ProjectIdLocal = @ProjectId
	SET @Today=dbo.GettingPMTime(GETUTCDATE())
    SET @CurrentMonthEnd =EOMONTH ( @Today )
	SET @LastMonthEnd=convert (date,DATEADD(MONTH, DATEDIFF(MONTH, -1, @Today)-1, -1))

			
		SELECT CC.ProjectId,
				TE.PersonId,
				TE.ChargeCodeDate,
				SUM(CASE WHEN TEH.IsChargeable = 1 THEN TEH.ActualHours ELSE 0 END) BillableHOursPerDay,
				SUM(CASE WHEN TEH.IsChargeable = 0 THEN TEH.ActualHours ELSE 0 END) NonBillableHoursPerDay,
				P.IsHourlyAmount
		INTO #ActualTimeEntriesToDate
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
		WHERE CC.ProjectId=@ProjectIdLocal AND (@ActualsEndDate IS NULL OR TE.ChargeCodeDate<=@ActualsEndDate)
		GROUP BY CC.ProjectId, TE.PersonId, TE.ChargeCodeDate,P.IsHourlyAmount

		CREATE Clustered index cix_ActualTimeEntriesToDate on #ActualTimeEntriesToDate(ProjectId, PersonId, ChargeCodeDate,IsHourlyAmount)
			
				
		SELECT f.ProjectId,
				f.Date,
				f.PersonMilestoneDailyAmount,
				f.PersonDiscountDailyAmount,
				(ISNULL(f.PayRate, 0) + ISNULL(f.OverheadRate, 0)+ISNULL(f.BonusRate,0)+ISNULL(f.VacationRate,0)) SLHR,
				ISNULL(f.PayRate, 0) PayRate,
				f.MLFOverheadRate,
				f.PersonHoursPerDay,
				f.PersonId,
				f.Discount,
				f.IsHourlyAmount,
				f.BillRate,
				f.ActualHoursPerDay,
				AE.BillableHOursPerDay,
      			AE.NonBillableHoursPerDay
		INTO #FinancialsRetro
		FROM v_FinancialsRetrospective f
		FULL JOIN #ActualTimeEntriesToDate AE on AE.ProjectId=f.ProjectId AND AE.PersonId = f.PersonId AND f.Date = AE.ChargeCodeDate
		WHERE f.ProjectId = @ProjectIdLocal 
				

		CREATE Clustered index cix_FinancialsRetro on #FinancialsRetro(ProjectId, PersonId, Date)
		 
		  SELECT	f.ProjectId,
					f.PersonId,
					f.Date,
					SUM(CASE WHEN f.IsHourlyAmount = 0  THEN f.PersonMilestoneDailyAmount ELSE 0 END ) AS FixedActualRevenuePerDay,
					(ISNULL(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.BillRate* f.ActualHoursPerDay ELSE 0 END),0) / ISNULL(NULLIF(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.ActualHoursPerDay ELSE 0 END),0),1)) * MAX(CASE WHEN f.IsHourlyAmount = 1 THEN  f.BillableHOursPerDay ELSE 0 END) HourlyActualRevenuePerDay,
					--SUM(CASE WHEN f.IsHourlyAmount = 0 and (@ActualsEndDate IS NULL OR f.Date<=@ActualsEndDate)  THEN 	f.PersonMilestoneDailyAmount ELSE 0 END)- (
								(ISNULL( MAX( CASE WHEN f.IsHourlyAmount = 0   THEN  f.BillableHOursPerDay + f.NonBillableHoursPerDay ELSE 0 END ),0) * 
								ISNULL( CASE WHEN ISNULL(SUM(CASE WHEN f.IsHourlyAmount = 0 THEN f.ActualHoursPerDay ELSE 0 END),0) > 0 
											THEN SUM((CASE WHEN f.SLHR >=  f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END)* CASE WHEN f.IsHourlyAmount = 0 THEN f.ActualHoursPerDay ELSE 0 END) / SUM(CASE WHEN f.IsHourlyAmount = 0 THEN f.ActualHoursPerDay ELSE 0 END)
											ELSE SUM((CASE WHEN f.SLHR >=  f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END)) / ISNULL(COUNT(f.PayRate),1)  
										END ,0)
							)
							 AS FixedActualCostPerDay,
						--((ISNULL(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.BillRate* f.ActualHoursPerDay ELSE 0 END),0) / ISNULL(NULLIF(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.ActualHoursPerDay ELSE 0 END),0),1)) * MAX(CASE WHEN f.IsHourlyAmount = 1 THEN  f.BillableHOursPerDay ELSE 0 END))
						-- -  (
							(	MAX( CASE WHEN f.IsHourlyAmount = 1 THEN  f.BillableHOursPerDay + f.NonBillableHoursPerDay ELSE 0 END ) * 
								ISNULL( CASE WHEN ISNULL(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.ActualHoursPerDay ELSE 0 END),0) > 0 
											THEN SUM((CASE WHEN f.SLHR >=  f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END)* CASE WHEN f.IsHourlyAmount = 1 THEN f.ActualHoursPerDay ELSE 0 END) / SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.ActualHoursPerDay ELSE 0 END)
											ELSE SUM((CASE WHEN f.SLHR >=  f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END)) / ISNULL(COUNT(f.PayRate),1)  
										END ,0)
							) AS HourlyActualCostPerDay,
					SUM(ISNULL(f.BillableHOursPerDay, 0)+ ISNULL(f.NonBillableHoursPerDay,0)) AS ActualHours,
					SUM(ISNULL(CASE WHEN (f.Date > @Today AND f.BillableHOursPerDay IS NULL AND f.NonBillableHoursPerDay IS NULL) THEN f.PersonHoursPerDay ELSE 0 END, 0)) as RemainingProjectedHours,
					SUM(CASE WHEN f.IsHourlyAmount = 0 AND @ActualsEndDate IS NOT NULL AND f.Date > EOMONTH(@ActualsEndDate) THEN f.PersonMilestoneDailyAmount 
							 WHEN f.IsHourlyAmount = 1 AND (f.Date > @Today AND f.BillableHOursPerDay IS NULL AND f.NonBillableHoursPerDay IS NULL) THEN f.PersonMilestoneDailyAmount
							 ELSE 0 END) AS RemainingProjectedRevenue,
					SUM(CASE WHEN f.IsHourlyAmount = 0 AND @ActualsEndDate IS NOT NULL AND f.Date > @ActualsEndDate THEN (CASE WHEN f.Date > EOMONTH(@ActualsEndDate) THEN (f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount) ELSE 0 END - (
																					CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate THEN f.SLHR 
																					ELSE f.PayRate + f.MLFOverheadRate END
																				) * ISNULL(f.PersonHoursPerDay, 0)) 
							 WHEN f.IsHourlyAmount = 1 AND (f.Date > @Today AND f.BillableHOursPerDay IS NULL AND f.NonBillableHoursPerDay IS NULL) THEN (f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount - (
																					CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate THEN f.SLHR 
																					ELSE f.PayRate + f.MLFOverheadRate END
																				) * ISNULL(f.PersonHoursPerDay, 0)) 
							ELSE 0 END) AS RemainingProjectedMargin
		INTO #ActualsValuesDaily
		FROM #FinancialsRetro AS f
		WHERE f.ProjectId=@ProjectIdLocal
		GROUP BY f.ProjectId, f.PersonId,f.Date,f.BillRate
	  
	   SELECT	CT.ProjectId,
				CT.PersonId,
				SUM(ISNULL(CT.HourlyActualRevenuePerDay, 0) + CASE WHEN @ActualsEndDate IS NULL OR CT.Date<= EOMONTH(@ActualsEndDate) THEN  ISNULL(CT.FixedActualRevenuePerDay, 0) ELSE 0 END) AS ActualRevenue,
				SUM(ISNULL(CT.HourlyActualRevenuePerDay, 0)- ISNULL(CT.HourlyActualCostPerDay,0) + CASE WHEN @ActualsEndDate IS NULL OR CT.Date<= EOMONTH(@ActualsEndDate) THEN  ISNULL(CT.FixedActualRevenuePerDay, 0) ELSE 0 END - ISNULL(CT.FixedActualCostPerDay,0)) as ActualMargin,
				SUM(CT.ActualHours) as ActualHours,
				SUM(ISNULL(CT.RemainingProjectedRevenue,0)) as RemainingProjectedRevenue,
				SUM(ISNULL(CT.RemainingProjectedMargin,0)) as RemainingProjectedMargin,
				SUM(CT.RemainingProjectedHours) as RemainingProjectedHours,
				SUM(ISNULL(CT.RemainingProjectedRevenue,0)+ ISNULL(CT.HourlyActualRevenuePerDay, 0) + CASE WHEN  @ActualsEndDate IS NULL OR CT.Date<= EOMONTH(@ActualsEndDate) THEN  ISNULL(CT.FixedActualRevenuePerDay, 0) ELSE 0 END) as EACRevenue,
				SUM(ISNULL(CT.RemainingProjectedMargin,0) + ISNULL(CT.HourlyActualRevenuePerDay, 0)- ISNULL(CT.HourlyActualCostPerDay,0) + CASE WHEN  @ActualsEndDate IS NULL OR CT.Date<=EOMONTH(@ActualsEndDate) THEN  ISNULL(CT.FixedActualRevenuePerDay, 0) ELSE 0 END - ISNULL(CT.FixedActualCostPerDay,0)) as EACMargin,
				SUM(CT.ActualHours + CT.RemainingProjectedHours) as EACHours
	    INTO #ActualAndProjected
		FROM #ActualsValuesDaily CT
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
				   ISNULL(SUM(f.PersonMilestoneDailyAmount),0) AS Revenue,
				   ISNULL(SUM(f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount -
								(CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate 
									  THEN f.SLHR  ELSE f.PayRate+f.MLFOverheadRate END) 
								*ISNULL(f.PersonHoursPerDay, 0)),0) BudgetMargin,
				   ISNULL(SUM(f.PersonHoursPerDay), 0) AS BudgetHours
			INTO #Budget
			FROM #BudgetRestrospective AS f
			WHERE f.ProjectId=@ProjectIdLocal AND f.PersonId IS NOT NULL AND(@BudgetToDate = 0 OR f.Date<=@Today)
			GROUP BY f.ProjectId, f.PersonId
	  

	

	  SELECT B.ProjectId,
			 B.PersonId,
			 Per.FirstName,
			 Per.LastName,
			 Per.TitleId,
			 Per.Title,
			 CONVERT(DECIMAL(6,2),B.BudgetHours) BudgetHours,
			 CONVERT(DECIMAL(18,2),B.BudgetMargin) as BudgetMargin,
			 CONVERT(DECIMAL(18,2),B.Revenue) as BudgetRevenue,
			 CONVERT(DECIMAL(6,2), ISNULL(A.ActualHours,0)) as ActualHours,
			 CONVERT(DECIMAL(18,2), ISNULL(A.ActualMargin,0)) as ActualMargin,
			 CONVERT(DECIMAL(18,2),ISNULL(A.ActualRevenue,0)) as ActualRevenue,
			 CONVERT(DECIMAL(6,2), ISNULL(A.RemainingProjectedHours,0)) as ProjectedHours,
			 CONVERT(DECIMAL(18,2),ISNULL(A.RemainingProjectedMargin,0)) as GrossMargin,
			 CONVERT(DECIMAL(18,2),ISNULL(A.RemainingProjectedRevenue,0)) as ProjectedRevenue,
			 CONVERT(DECIMAL(6,2), ISNULL(A.EACHours,0)) as EACHours,
			 CONVERT(DECIMAL(18,2),ISNULL(A.EACMargin,0)) as EACMargin,
			 CONVERT(DECIMAL(18,2),ISNULL(A.EACRevenue,0)) as EACRevenue
	  FROM #Budget B
	  LEFT JOIN #ActualAndProjected A on A.PersonId=B.PersonId 
	  LEFT JOIN v_Person Per on B.PersonId=Per.PersonId

	;WITH ActualExpenses
	AS
	(
	SELECT PDE.ProjectId,
		   SUM( PDE.ActualExpense) as ActualExpense
	FROM v_ProjectDailyExpenses PDE
	WHERE PDE.ProjectId =@ProjectIdLocal AND (@ActualsEndDate IS NULL OR PDE.Date <= @ActualsEndDate)
	GROUP BY PDE.ProjectId
	),

	RemainingProjectedExpenses
	AS
	(
	SELECT PDE.ProjectId,
		   SUM(PDE.EstimatedAmount) as EstimatedAmount
	FROM v_ProjectDailyExpenses PDE
	WHERE PDE.ProjectId =@ProjectIdLocal AND @ActualsEndDate IS NOT NULL AND PDE.Date > @ActualsEndDate
	GROUP BY PDE.ProjectId
	)

	  SELECT PBH.ProjectId,
		      CONVERT(DECIMAL(18,2),PBH.Expense) AS BudgetExpense,
			 CONVERT(DECIMAL(18,2),ISNULL(A.ActualExpense, 0)) ActualExpense,
			 CONVERT(DECIMAL(18,2),ISNULL(P.EstimatedAmount,0)) as ProjectedExpense
	  FROM ProjectBudgetHistory PBH
	  full JOIN ActualExpenses A on A.ProjectId=PBH.ProjectId
	  full JOIN RemainingProjectedExpenses P on P.ProjectId=PBH.ProjectId
	  WHERE PBH.ProjectId=@ProjectIdLocal AND PBH.MilestoneId IS NULL AND PBH.IsActive=1

	  DROP TABLE #ActualAndProjected
	  DROP TABLE #ActualsValuesDaily
	  DROP TABLE #ActualTimeEntriesToDate
	  DROP TABLE #Budget
	  DROP TABLE #FinancialsRetro

	  END 
	  



