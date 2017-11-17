CREATE VIEW [dbo].[v_ProjectTotalExpenses]
AS
	SELECT 
		ProjectId,
		CONVERT(DECIMAL(18,2),sum(ExpectedAmount)) as 'ExpenseSum',
		CONVERT(DECIMAL(18,2),sum(ExpectedAmount*Reimbursement*0.01)) as 'ReimbursedExpenseSum'
	FROM ProjectExpense
	GROUP BY ProjectId