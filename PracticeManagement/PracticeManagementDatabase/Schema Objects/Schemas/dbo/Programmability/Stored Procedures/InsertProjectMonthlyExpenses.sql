CREATE PROCEDURE [dbo].[InsertProjectMonthlyExpenses]
(
	@ExpenseId	INT,
	@StartDate	DATETIME,
	@EndDate	DATETIME,
	@EstimatedAmount	DECIMAL(18,2),
	@ActualAmount	DECIMAL(18,2)
)
AS
	SET NOCOUNT ON

	
	INSERT INTO ProjectMonthlyExpense(ExpenseId,StartDate,EndDate, EstimatedAmount,ActualAmount)
	VALUES(@ExpenseId,@StartDate,@EndDate, @EstimatedAmount,@ActualAmount)

