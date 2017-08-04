CREATE VIEW [dbo].[v_FinancialRestospectiveETC]
AS

    WITH FinancialsRetro 
	AS 
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
	FROM v_FinancialsRetrospectiveActualHours f
	),


	RemainingProjectedExpenses
	AS
	(
	SELECT PDE.ProjectId,
		   PDE.Date,
		   SUM(PDE.EstimatedAmount) as EstimatedAmount,
		   SUM(PDE.EstimatedReimbursement) as EstimatedReimbursement
	FROM v_ProjectDailyExpenses PDE
	WHERE  PDE.Date> dbo.GettingPMTime(GETUTCDATE())
	GROUP BY PDE.ProjectId, PDE.Date
	),

	RemainingProjectedFinancials
	AS
	(
	SELECT ISNULL(f.ProjectId,RE.ProjectId) ProjectId, 
		   ISNULL(f.Date,RE.Date) as Date,
	       SUM(f.PersonMilestoneDailyAmount) AS Revenue,
	       SUM(f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount) AS RevenueNet,
		   SUM(f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount -
						(CASE WHEN f.SLHR  >= f.PayRate + f.MLFOverheadRate 
							  THEN f.SLHR ELSE f.PayRate + f.MLFOverheadRate END) 
					    *ISNULL(f.PersonHoursPerDay, 0)) GrossMargin,
		   ISNULL(SUM((CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE f.PayRate + f.MLFOverheadRate END)*ISNULL(f.PersonHoursPerDay, 0)),0) Cogs,
	       ISNULL(SUM(f.PersonHoursPerDay), 0) AS Hours,
		   ISNULL(SUM(RE.EstimatedAmount), 0) As EstimatedExpense,
		   ISNULL(SUM(RE.EstimatedReimbursement), 0) As EstimatedReimbursement,
		   min(f.Discount) as Discount
 	  FROM FinancialsRetro AS f
	  FULL JOIN RemainingProjectedExpenses RE on f.projectId=RE.ProjectId AND f.Date=RE.Date
	 WHERE f.Date > dbo.GettingPMTime(GETUTCDATE())
	GROUP BY ISNULL(f.ProjectId,RE.ProjectId), ISNULL(f.Date,RE.Date)
	),


	ActualValuesDaily
	AS
	(
	SELECT	f.ProjectId,
			f.Date,
			SUM(CASE WHEN (f.IsHourlyAmount = 0 and f.Date<= EOMONTH( dbo.GettingPMTime(GETUTCDATE()))) THEN f.PersonMilestoneDailyAmount ELSE 0 END ) AS FixedActualRevenuePerDay,
			(ISNULL(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.BillRate* f.ActualHoursPerDay ELSE 0 END),0) / ISNULL(NULLIF(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.ActualHoursPerDay ELSE 0 END),0),1)) * MAX(CASE WHEN f.IsHourlyAmount = 1 THEN  f.BillableHOursPerDay ELSE 0 END) HourlyActualRevenuePerDay,
			ISNULL(SUM(CASE WHEN (f.IsHourlyAmount = 0 and f.Date<= EOMONTH( dbo.GettingPMTime(GETUTCDATE()))) THEN 	f.PersonMilestoneDailyAmount ELSE 0 END),0)- (
						ISNULL( MAX( CASE WHEN f.IsHourlyAmount = 0 THEN  f.BillableHOursPerDay + f.NonBillableHoursPerDay ELSE 0 END ),0) * 
						ISNULL( CASE WHEN ISNULL(SUM(CASE WHEN f.IsHourlyAmount = 0 THEN f.ActualHoursPerDay ELSE 0 END),0) > 0 
									THEN SUM((CASE WHEN f.SLHR >=  f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END)* CASE WHEN f.IsHourlyAmount = 0 THEN f.ActualHoursPerDay ELSE 0 END) / SUM(CASE WHEN f.IsHourlyAmount = 0 THEN f.ActualHoursPerDay ELSE 0 END)
									ELSE SUM((CASE WHEN f.SLHR >=  f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END)) / ISNULL(COUNT(f.PayRate),1)  
								END ,0)
					) AS FixedActualMarginPerDay,
			((ISNULL(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.BillRate* f.ActualHoursPerDay ELSE 0 END),0) / ISNULL(NULLIF(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.ActualHoursPerDay ELSE 0 END),0),1)) * MAX(CASE WHEN f.IsHourlyAmount = 1 THEN  f.BillableHOursPerDay ELSE 0 END))
				 -  (
						MAX( CASE WHEN f.IsHourlyAmount = 1 THEN  f.BillableHOursPerDay + f.NonBillableHoursPerDay ELSE 0 END ) * 
						ISNULL( CASE WHEN ISNULL(SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.ActualHoursPerDay ELSE 0 END),0) > 0 
									THEN SUM((CASE WHEN f.SLHR >=  f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END)* CASE WHEN f.IsHourlyAmount = 1 THEN f.ActualHoursPerDay ELSE 0 END) / SUM(CASE WHEN f.IsHourlyAmount = 1 THEN f.ActualHoursPerDay ELSE 0 END)
									ELSE SUM((CASE WHEN f.SLHR >=  f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END)) / ISNULL(COUNT(f.PayRate),1)  
								END ,0)
					) AS HourlyActualMarginPerDay,
			ISNULL(SUM(f.PersonHoursPerDay), 0) AS ProjectedHoursPerDay,
		  	min(f.Discount) as Discount
	FROM FinancialsRetro AS f
	GROUP BY f.ProjectId, f.Date
	),



	ActualExpenses
	AS
	(
	SELECT PDE.ProjectId,
		   PDE.Date,
		   SUM(CASE WHEN PDE.Date<= dbo.GettingPMTime(GETUTCDATE()) THEN  PDE.ActualExpense ELSE 0 END) as ActualExpense,
		   SUM(CASE WHEN PDE.Date<=dbo.GettingPMTime(GETUTCDATE()) THEN  PDE.ActualReimbursement ELSE 0 END) as ActualReimbursement
	FROM v_ProjectDailyExpenses PDE
	WHERE  PDE.Date<= dbo.GettingPMTime(GETUTCDATE())
	GROUP BY PDE.ProjectId, PDE.Date
	),


	Actuals
	AS
	(SELECT ISNULL(CT.ProjectId,AE.ProjectId) ProjectId, 
			ISNULL(CT.Date,AE.Date) as Date,
			SUM(ISNULL(CT.HourlyActualRevenuePerDay, 0) + ISNULL(CT.FixedActualRevenuePerDay, 0)) AS ActualRevenue,
			SUM(ISNULL(CT.HourlyActualMarginPerDay, 0) + ISNULL(CT.FixedActualMarginPerDay, 0)) as ActualMargin,
			MAX(ISNULL(CT.Discount, 0)) Discount,
			SUM(ISNULL(AE.ActualExpense, 0)) ActualExpense,
			SUM(ISNULL(AE.ActualReimbursement, 0)) ActualReimbursement
	FROM ActualValuesDaily CT
	Full JOIN ActualExpenses AE on CT.projectId=AE.ProjectId AND CT.Date=AE.Date
	GROUP BY ISNULL(CT.ProjectId,AE.ProjectId), ISNULL(CT.Date,AE.Date)
	)

	SELECT
	    ISNULL(A.ProjectId, P.ProjectId) as ProjectId,
		ISNULL(A.Date,P.Date) as Date,
		CONVERT(DECIMAL(18,6), (ISNULL(A.ActualRevenue,0)+ISNULL(P.Revenue,0))) ETCRevenue,
		CONVERT(DECIMAL(18,6), ((ISNULL(A.ActualRevenue,0)- (ISNULL(A.ActualRevenue,0) * ISNULL(A.Discount,0)/100))+ISNULL(P.RevenueNet,0))) ETCRevenueNet,
		CONVERT(DECIMAL(18,6), ((ISNULL(A.ActualMargin,0)- (ISNULL(A.ActualRevenue,0) * ISNULL(A.Discount,0)/100)+ISNULL(A.ActualReimbursement,0)-ISNULL(A.ActualExpense,0))+
		ISNULL(P.GrossMargin,0)+ISNULL(P.EstimatedReimbursement,0)-ISNULL(P.EstimatedExpense,0))) ETCGrossMargin,
		ISNULL(A.ActualExpense,0)+ISNULL(P.EstimatedExpense,0) as Expense,
		ISNULL(A.ActualReimbursement,0)+ISNULL(P.EstimatedReimbursement,0) as ExpenseReimbursement
	FROM Actuals A
	FULL JOIN RemainingProjectedFinancials P ON A.ProjectId=P.ProjectId AND A.Date =P.Date


