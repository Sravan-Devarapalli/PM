﻿CREATE PROCEDURE dbo.FinancialsGetByMilestonePersonsMonthly
(
	@MilestoneId      INT
)
AS
	SET NOCOUNT ON
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
		   f.Discount,
		   f.EntryId
	FROM v_FinancialsRetrospective f
	WHERE f.MilestoneId = @MilestoneIdLocal
	)
	SELECT f.ProjectId,
		   f.EntryId,
		   f.PersonId,
		   MPE.StartDate,
		   MPE.EndDate,
	       C.MonthStartDate AS FinancialDate,
	       C.MonthEndDate AS MonthEnd,
	       ISNULL(SUM(f.PersonMilestoneDailyAmount), 0) AS Revenue,
		   ISNULL(SUM(f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount), 0) AS RevenueNet,
	       ISNULL(SUM((CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END)*ISNULL(f.PersonHoursPerDay, 0)),0) Cogs,
	       ISNULL(SUM(f.PersonMilestoneDailyAmount - f.PersonDiscountDailyAmount -
						(CASE WHEN f.SLHR >= f.PayRate+f.MLFOverheadRate 
							  THEN f.SLHR ELSE  f.PayRate +f.MLFOverheadRate END) 
					    *ISNULL(f.PersonHoursPerDay, 0)), 0) GrossMargin,

	       ISNULL(SUM(f.PersonHoursPerDay), 0) AS Hours

	  FROM FinancialsRetro AS f
	  INNER JOIN dbo.Calendar C ON C.Date = f.Date
	  JOIN MilestonePersonEntry MPE ON f.EntryId = MPE.Id AND f.Date BETWEEN MPE.StartDate AND MPE.EndDate
	 WHERE f.MilestoneId = @MilestoneIdLocal AND f.PersonId IS NOT NULL
	GROUP BY f.ProjectId,f.EntryId, f.PersonId, MPE.StartDate, MPE.EndDate, C.MonthStartDate, C.MonthEndDate

