CREATE PROCEDURE [dbo].[UpdateProjectMonthlyExpenses]
(
	@Id	INT,
	@ExpenseId	INT,
	@StartDate	DATETIME,
	@EndDate	DATETIME,
	@EstimatedAmount	DECIMAL(18,2),
	@ActualAmount	DECIMAL(18,2)
)
AS
	SET NOCOUNT ON

	UPDATE PME
	SET	PME.StartDate=@StartDate,
		PME.EndDate=@EndDate,
		PME.EstimatedAmount =@EstimatedAmount,
		PME.ActualAmount=@ActualAmount
	FROM ProjectMonthlyExpense PME
	WHERE PME.Id=@Id AND PME.ExpenseId=@ExpenseId


