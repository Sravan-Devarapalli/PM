CREATE PROCEDURE [dbo].[GetForecastDataForMilestone]
(
	@MilestoneId INT
)
AS 
BEGIN
	SET NOCOUNT ON;

		IF OBJECT_ID('#ProjectedValuesDaily') IS NOT NULL
        DROP TABLE #ProjectedValuesDaily

		IF OBJECT_ID('#ProjectedMonthly') IS NOT NULL
        DROP TABLE #ProjectedMonthly

		IF OBJECT_ID('#PersonBillRatePeriods') IS NOT NULL
        DROP TABLE #PersonBillRatePeriods

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
				f.EntryId,
				f.BillRate
		FROM v_FinancialsRetrospective f
		WHERE f.MilestoneId = @MilestoneId
		)
		
		

			SELECT	f.ProjectId,
					f.Date,
					f.PersonId,
					CONVERT(DECIMAL(18,2),f.BillRate) BillRate,
					CONVERT(DECIMAL(18,2),ISNULL(SUM(CASE WHEN f.SLHR >= f.PayRate + f.MLFOverheadRate THEN f.SLHR ELSE  f.PayRate + f.MLFOverheadRate END),0)) as PersonCost,
					ISNULL(SUM(f.PersonHoursPerDay), 0) AS ProjectedHoursPerDay
			INTO #ProjectedValuesDaily
			FROM FinancialsRetro AS f
			GROUP BY f.ProjectId,f.PersonId, f.Date, f.BillRate
			

			
			SELECT A.ProjectId,
				   A.PersonId,
				   C.MonthStartDate AS FinancialDate, 
				   CONVERT(DECIMAL(6,2),SUM(ISNULL(A.ProjectedHoursPerDay,0))) as ProjectedHours
			INTO #ProjectedMonthly
			FROM #ProjectedValuesDaily A
			JOIN dbo.Calendar C on c.Date=A.Date
			GROUP BY A.ProjectId, A.PersonId, C.MonthStartDate, C.MonthEndDate, C.MonthNumber
		 
		
			SELECT A.PersonId,
				   MIN(A.Date) as StartDate,
				   A.BillRate,
				   A.PersonCost as PayRate
			INTO #PersonBillRatePeriods
			FROM #ProjectedValuesDaily A
			WHERE A.BillRate IS NOT NULL AND A.BillRate!=0
			GROUP BY A.PersonId, A.BillRate, A.PersonCost
		
		    select A.PersonId,
				   P.FirstName,
				   P.LastName,
				   T.TitleId,
				   T.Title,
				   A.FinancialDate,
				   A.ProjectedHours
			from #ProjectedMonthly A
			JOIN dbo.Person P on A.PersonId = P.PersonId
			LEFT JOIN dbo.Title AS T ON p.TitleId = T.TitleId 
			ORDER BY P.LastName, A.FinancialDate



			SELECT * FROM #PersonBillRatePeriods
			ORDER BY StartDate
END
