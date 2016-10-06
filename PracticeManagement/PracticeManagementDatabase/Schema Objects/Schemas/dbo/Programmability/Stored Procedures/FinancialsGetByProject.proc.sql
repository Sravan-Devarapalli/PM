﻿CREATE PROCEDURE dbo.FinancialsGetByProject
(
	@ProjectId   INT
)
AS
	SET NOCOUNT ON;

	DECLARE @ProjectIdLocal	INT

	SELECT @ProjectIdLocal = @ProjectId

	;WITH FinancialsRetro AS 
	(
	SELECT f.ProjectId,
		   f.Date, 
		   f.PersonMilestoneDailyAmount,
		   f.PersonDiscountDailyAmount,
		   (ISNULL(f.PayRate, 0) + ISNULL(f.OverheadRate, 0)+ISNULL(f.BonusRate,0)+ISNULL(f.VacationRate,0)) SLHR,
		   ISNULL(f.PayRate, 0) PayRate,
		   f.MLFOverheadRate,
		   f.PersonHoursPerDay,
		   f.PersonId,
		   f.Discount
	FROM v_FinancialsRetrospective f
	WHERE f.ProjectId = @ProjectIdLocal
	),
	
	ProjectFinancials
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
 	  FROM FinancialsRetro AS f
	 WHERE f.ProjectId = @ProjectIdLocal
	GROUP BY f.ProjectId
	)
	SELECT
		P.ProjectId,
		P.StartDate FinancialDate,
		p.EndDate MonthEnd,
		ISNULL(pf.Revenue,0)  as 'Revenue',
		ISNULL(pf.RevenueNet,0)+ISNULL(PE.ReimbursedExpenseSum,0) as 'RevenueNet',
		CASE WHEN (pr.IsCompanyInternal = 1) THEN 0
		ELSE ISNULL(pf.Cogs,0) END AS 'Cogs',
		ISNULL(pf.GrossMargin,0)+((ISNULL(PE.ReimbursedExpenseSum,0)-ISNULL(PE.ExpenseSum,0)))  as 'GrossMargin',
		ISNULL(pf.Hours,0) Hours,
		ISNULL(PE.ExpenseSum,0) Expense,
		ISNULL(PE.ReimbursedExpenseSum,0) ReimbursedExpense
	FROM  Project p 
	JOIN Practice pr ON (pr.PracticeId = p.PracticeId)
	LEFT JOIN ProjectFinancials pf
	ON (p.ProjectId = pf.ProjectId)
	LEFT JOIN v_ProjectTotalExpenses PE ON P.ProjectId = PE.ProjectId
	WHERE (pf.ProjectId IS NOT NULL OR PE.ProjectId IS NOT NULL) AND P.ProjectId =@ProjectIdLocal
			AND P.StartDate IS NOT  NULL AND P.EndDate IS NOT NULL	
	

