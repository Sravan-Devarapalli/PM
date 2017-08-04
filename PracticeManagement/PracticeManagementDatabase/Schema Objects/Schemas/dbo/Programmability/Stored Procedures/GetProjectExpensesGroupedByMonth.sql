CREATE PROCEDURE [dbo].[GetProjectExpensesGroupedByMonth]
(
	@ProjectId INT,
	@StartDate DATETIME = NULL,
	@EndDate DATETIME =NULL,
	@MilestoneId INT = NULL
)
AS
BEGIN
SET NOCOUNT ON;
		DECLARE @StartDateLocal DATETIME = NULL ,
				@EndDateLocal DATETIME = NULL ,
				@ProjectIdLocal INT = NULL ,
				@Today DATE,
				@MilestoneIdLocal INT = NULL

			   SELECT @ProjectIdLocal = @ProjectId, @MilestoneIdLocal= @MilestoneId


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
					     IF(@MilestoneIdLocal IS NULL)
						 BEGIN
							 SELECT @startDateLocal= p.StartDate, @endDateLocal=p.EndDate
							 FROM Project P
							 WHERE P.ProjectId=@ProjectId
						 END
						 ELSE
						 BEGIN
						     SELECT @StartDateLocal=M.StartDate, @EndDateLocal = M.ProjectedDeliveryDate
							 FROM dbo.Milestone M
							 WHERE M.MilestoneId = @MilestoneIdLocal
						END
					END

		-- Projected and Actual Expenses

	    SELECT PDE.ProjectId,
			   PDE.ExpenseTypeId,
			   PDE.ExpenseTypeName,
			   PDE.FinancialDate,
			   SUM(PDE.EstimatedAmount) as ProjectedExpense,
			   SUM(CASE WHEN (PDE.date>@Today) THEN PDE.EstimatedAmount ELSE 0 END) as RemainingProjectedExpense,
			   SUM(CASE WHEN (PDE.date <= @Today) THEN PDE.ActualExpense ELSE 0 END) as ActualExpense
		FROM v_ProjectDailyExpenses PDE
		WHERE PDE.ProjectId = @ProjectIdLocal AND (@MilestoneIdLocal IS NULL OR @MilestoneIdLocal=PDE.MilestoneId) AND PDE.Date BETWEEN @StartDateLocal AND @EndDateLocal
		GROUP BY PDE.ProjectId, PDE.ExpenseTypeId,PDE.ExpenseTypeName, PDE.FinancialDate
		ORDER BY PDE.FinancialDate

		-- Budget Expenses

		SELECT  pexp.ProjectId,
				ET.Id as ExpenseTypeId,
				ET.Name as ExpenseTypeName,
			    C.MonthStartDate AS FinancialDate,
			    CONVERT(DECIMAL(18,2),SUM(ISNULL(pexp.Amount,0))) BudgetExpense
		FROM v_ProjectBudgetDailyExpenses pexp
		--FROM dbo.ProjectBudgetExpense as pexp
		JOIN dbo.ExpenseType ET on ET.Id=pexp.ExpenseTypeId
		JOIN dbo.Calendar c ON c.Date = pexp.date
		WHERE pexp.ProjectId=@ProjectIdLocal AND (@MilestoneId IS NULL OR @MilestoneId=pexp.MilestoneId) AND  c.Date BETWEEN @StartDateLocal AND @EndDateLocal
		GROUP BY pexp.ProjectId, ET.Id, ET.Name,  C.MonthStartDate
		ORDER BY C.MonthStartDate
END



