CREATE PROCEDURE [dbo].[GetMonthlyExpensesByExpenseId]
(
	@ExpenseId	INT
)
AS
	SET NOCOUNT ON

	SELECT PME.Id,
		   PME.ExpenseId,
		   PME.StartDate,
		   PME.EndDate,
		   PME.EstimatedAmount,
		   PME.ActualAmount
	FROM ProjectMonthlyExpense PME
	WHERE PME.ExpenseId=@ExpenseId
