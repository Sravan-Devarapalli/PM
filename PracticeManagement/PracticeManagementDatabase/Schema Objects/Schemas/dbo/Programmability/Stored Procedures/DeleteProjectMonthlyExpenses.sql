CREATE PROCEDURE [dbo].[DeleteProjectMonthlyExpenses]
(
	@ExpenseId	INT
)
AS
	SET NOCOUNT ON

	DELETE FROM ProjectMonthlyExpense
	WHERE ExpenseId=@ExpenseId
