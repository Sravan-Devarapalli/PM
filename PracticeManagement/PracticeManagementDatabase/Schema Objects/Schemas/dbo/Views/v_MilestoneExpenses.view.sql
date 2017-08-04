CREATE VIEW dbo.v_MilestoneExpenses
AS
	SELECT pexp.MilestoneId,
	pexp.projectid,
		  CONVERT(DECIMAL(18,2),SUM(pexp.ExpectedAmount)) Expense,
		  CONVERT(DECIMAL(18,2),SUM(pexp.Reimbursement*0.01*pexp.ExpectedAmount)) ReimbursedExpense 
	FROM dbo.ProjectExpense as pexp 
	GROUP BY pexp.MilestoneId,
	pexp.projectid
