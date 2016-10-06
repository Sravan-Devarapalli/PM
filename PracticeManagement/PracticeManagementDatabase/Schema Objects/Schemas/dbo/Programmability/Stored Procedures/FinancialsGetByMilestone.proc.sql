﻿CREATE PROCEDURE dbo.FinancialsGetByMilestone
(
	@MilestoneId      INT
)
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @MilestoneIdLocal INT
	SELECT @MilestoneIdLocal = @MilestoneId
	;WITH FinancialsRetro AS 
	(
	SELECT f.ProjectId,
		   f.MilestoneId,
		   f.Date, 
		   f.PersonMilestoneDailyAmount,
		   f.PersonDiscountDailyAmount,
		   (ISNULL(f.PayRate, 0) + ISNULL(f.OverheadRate, 0)+ISNULL(f.BonusRate,0)+ISNULL(f.VacationRate,0)) SLHR,
		   ISNULL(f.PayRate,0) PayRate,
		   f.MLFOverheadRate,
		   f.PersonHoursPerDay,
		   f.PersonId,
		   f.Discount
	FROM v_FinancialsRetrospective f
	WHERE f.MilestoneId = @MilestoneIdLocal
	),
	MilestoneFinancials as 
	(SELECT f.ProjectId,
			f.MilestoneId,
	       MIN(C.MonthStartDate) AS FinancialDate,
	       MIN(C.MonthEndDate) AS MonthEnd,

	       SUM(f.PersonMilestoneDailyAmount) AS Revenue,
		   SUM(f.PersonDiscountDailyAmount) As DiscountAmount,
	       SUM(f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount) AS RevenueNet,
	       
		   SUM(f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount -
						(CASE WHEN f.SLHR >=  f.PayRate +f.MLFOverheadRate 
							  THEN f.SLHR ELSE f.PayRate +f.MLFOverheadRate END) 
					    *ISNULL(f.PersonHoursPerDay, 0)) GrossMargin,
		   ISNULL(SUM((CASE WHEN f.SLHR >=  f.PayRate +f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate +f.MLFOverheadRate END)*ISNULL(f.PersonHoursPerDay, 0)),0) Cogs,
	       ISNULL(SUM(f.PersonHoursPerDay), 0) AS Hours
	  FROM FinancialsRetro AS f
	  INNER JOIN dbo.Calendar C ON C.Date = f.Date
	 WHERE f.MilestoneId = @MilestoneIdLocal
	GROUP BY f.ProjectId,f.MilestoneId
	)

	select
		M.ProjectId,
		ISNULL(f.FinancialDate,M.StartDate) FinancialDate,
		ISNULL(f.MonthEnd,M.ProjectedDeliveryDate) MonthEnd,
		ISNULL(f.Revenue,0) as 'Revenue',
		ISNULL(RevenueNet,0)+ISNULL(Me.ReimbursedExpense,0) as 'RevenueNet',
		ISNULL(Cogs,0) Cogs,
		ISNULL(GrossMargin,0)+((ISNULL(Me.ReimbursedExpense,0) -ISNULL(ME.Expense,0))) as 'GrossMargin',
		ISNULL(Hours,0) Hours,
		ISNULL(ME.Expense,0) Expense,
		ISNULL(Me.ReimbursedExpense,0) ReimbursedExpense
	FROM Milestone M
	JOIN Project P ON P.ProjectId = M.ProjectId
	LEFT JOIN  MilestoneFinancials f ON f.MilestoneId = M.MilestoneId
	LEFT JOIN v_MilestoneExpenses ME ON Me.MilestoneId = M.MilestoneId
	WHERE M.MilestoneId = @MilestoneIdLocal AND (f.MilestoneId IS NOT NULL OR ME.MilestoneId IS NOT NULL)
	
END

