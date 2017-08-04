CREATE PROCEDURE [dbo].[UpdateProjectExpenseActualAmount]
(
	@ExpenseId	INT
)
AS
	SET NOCOUNT ON

	DECLARE @ActualAmount DECIMAL(18,2),
			@EstAmount  DECIMAL(18,2)

	select @ActualAmount= SUM(ISNULL(PME.ActualAmount,0)),
	@EstAmount=SUM(ISNULL(PME.EstimatedAmount,0))
	FROM ProjectMonthlyExpense PME
	WHERE PME.ExpenseId=@ExpenseId
	
	UPDATE PE
	SET PE.Amount=@ActualAmount,
		PE.ExpectedAmount=@EstAmount
	FROM ProjectExpense PE
	WHERE PE.Id = @ExpenseId


