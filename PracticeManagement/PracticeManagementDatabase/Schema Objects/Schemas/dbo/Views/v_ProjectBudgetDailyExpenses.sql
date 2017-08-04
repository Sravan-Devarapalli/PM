CREATE VIEW [dbo].[v_ProjectBudgetDailyExpenses]
AS
	SELECT  pexp.ExpenseId,
		    PE.Name,
			PE.ExpenseTypeId,
			ET.Name as ExpenseTypeName,
			PE.ProjectId,
			Pe.MilestoneId,
			pexp.Amount/(DATEDIFF(dd,pexp.StartDate,pexp.EndDate)+1) Amount,
			(ISNULL(pexp.Amount,0)*pe.Reimbursement*0.01)/(DATEDIFF(dd,pexp.StartDate,pexp.EndDate)+1) Reimbursement,
			c.date,
			C.MonthStartDate AS FinancialDate,
			C.MonthEndDate AS MonthEnd,
			C.MonthNumber
	FROM dbo.ProjectBudgetMonthlyExpense as pexp
	JOIN dbo.ProjectBudgetExpense as PE on pexp.ExpenseId=pe.Id
	JOIN ExpenseType ET on ET.Id=PE.ExpenseTypeId
	JOIN dbo.Calendar c ON c.Date BETWEEN pexp.StartDate AND pexp.EndDate

	UNION ALL

	SELECT DISTINCT PE.Id,
			PE.Name,
			PE.ExpenseTypeId,
			ET.Name as ExpenseTypeName,
			PE.ProjectId,
			Pe.MilestoneId,
			ISNULL(PE.Amount,0)/(DATEDIFF(dd,PE.StartDate,PE.EndDate)+1) Amount,
			(ISNULL(PE.Amount,0)*pe.Reimbursement*0.01)/(DATEDIFF(dd,pe.StartDate,pe.EndDate)+1) Reimbursement,
			c.date,
			C.MonthStartDate AS FinancialDate,
			C.MonthEndDate AS MonthEnd,
			C.MonthNumber
	FROM dbo.ProjectBudgetExpense as PE 
	LEFT JOIN (select distinct expenseid from dbo.ProjectBudgetMonthlyExpense) as pexp on pexp.ExpenseId=pe.Id
	JOIN dbo.Calendar c ON c.Date BETWEEN pe.StartDate AND pe.EndDate 
	JOIN ExpenseType ET on ET.Id=PE.ExpenseTypeId
	WHERE pexp.ExpenseId IS NULL 
