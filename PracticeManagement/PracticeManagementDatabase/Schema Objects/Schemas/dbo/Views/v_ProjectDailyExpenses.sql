CREATE VIEW [dbo].[v_ProjectDailyExpenses]
AS 

	SELECT  pexp.ExpenseId,
		    PE.Name,
			PE.ExpenseTypeId,
			ET.Name as ExpenseTypeName,
			PE.ProjectId,
			Pe.MilestoneId,
			ISNULL(pexp.ActualAmount,0)/(DATEDIFF(dd,pexp.StartDate,pexp.EndDate)+1) ActualExpense,
			pexp.EstimatedAmount/(DATEDIFF(dd,pexp.StartDate,pexp.EndDate)+1) EstimatedAmount,
			(ISNULL(pexp.ActualAmount,0)*pe.Reimbursement*0.01)/(DATEDIFF(dd,pexp.StartDate,pexp.EndDate)+1) ActualReimbursement,
			(pexp.EstimatedAmount*pe.Reimbursement*0.01)/(DATEDIFF(dd,pexp.StartDate,pexp.EndDate)+1) EstimatedReimbursement,
			c.date,
			C.MonthStartDate AS FinancialDate,
			C.MonthEndDate AS MonthEnd,
			C.MonthNumber
	FROM dbo.projectmonthlyExpense as pexp
	JOIN dbo.ProjectExpense as PE on pexp.ExpenseId=pe.Id
	JOIN ExpenseType ET on ET.Id=PE.ExpenseTypeId
	JOIN dbo.Calendar c ON c.Date BETWEEN pexp.StartDate AND pexp.EndDate

	UNION ALL

	SELECT DISTINCT PE.Id,
			PE.Name,
			PE.ExpenseTypeId,
			ET.Name as ExpenseTypeName,
			PE.ProjectId,
			Pe.MilestoneId,
			ISNULL(PE.Amount,0)/(DATEDIFF(dd,PE.StartDate,PE.EndDate)+1) Expense,
			PE.ExpectedAmount/(DATEDIFF(dd,PE.StartDate,PE.EndDate)+1) EstimatedAmount,
			(ISNULL(PE.Amount,0)*pe.Reimbursement*0.01)/(DATEDIFF(dd,pe.StartDate,pe.EndDate)+1) ActualReimbursement,
			(PE.ExpectedAmount*pe.Reimbursement*0.01)/(DATEDIFF(dd,pe.StartDate,pe.EndDate)+1) EstimatedReimbursement,
			c.date,
			C.MonthStartDate AS FinancialDate,
			C.MonthEndDate AS MonthEnd,
			C.MonthNumber
	FROM dbo.ProjectExpense as PE 
	LEFT JOIN (select distinct expenseid from dbo.projectmonthlyExpense) as pexp on pexp.ExpenseId=pe.Id
	JOIN dbo.Calendar c ON c.Date BETWEEN pe.StartDate AND pe.EndDate 
	JOIN ExpenseType ET on ET.Id=PE.ExpenseTypeId
	WHERE pexp.ExpenseId IS NULL 
