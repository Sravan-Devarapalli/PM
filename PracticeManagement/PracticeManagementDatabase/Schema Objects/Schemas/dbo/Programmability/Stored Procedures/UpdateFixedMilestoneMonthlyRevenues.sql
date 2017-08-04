CREATE PROCEDURE [dbo].[UpdateFixedMilestoneMonthlyRevenues]
(
	@Id	INT,
	@MilestoneId	INT,
	@StartDate	DATETIME,
	@EndDate	DATETIME,
	@Amount 	DECIMAL(18,2)
)
AS
	SET NOCOUNT ON

	UPDATE FMR
	SET	FMR.StartDate=@StartDate,
		FMR.EndDate=@EndDate,
		FMR.Amount =@Amount
	FROM FixedMilestoneMonthlyRevenue FMR
	WHERE FMR.Id=@Id AND FMR.MilestoneId=@MilestoneId
