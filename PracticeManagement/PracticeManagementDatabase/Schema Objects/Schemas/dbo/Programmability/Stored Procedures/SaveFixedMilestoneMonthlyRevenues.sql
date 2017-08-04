CREATE PROCEDURE [dbo].[SaveFixedMilestoneMonthlyRevenues]
(
	@MilestoneId	INT,
	@StartDate	DATETIME,
	@EndDate	DATETIME,
	@Amount	DECIMAL(18,2)
)
AS
	SET NOCOUNT ON

	
	INSERT INTO FixedMilestoneMonthlyRevenue(MilestoneId,StartDate,EndDate,Amount)
	VALUES(@MilestoneId,@StartDate,@EndDate,@Amount)

