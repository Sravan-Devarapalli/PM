CREATE PROCEDURE [dbo].[GetMonthlyRevenuesForMilestone]
(
	@MilestoneId	INT
)
AS
	SET NOCOUNT ON

	SELECT FMR.Id,
		   FMR.MilestoneId,
		   FMR.StartDate,
		   FMR.EndDate,
		   FMR.Amount
	FROM FixedMilestoneMonthlyRevenue FMR
	WHERE FMR.MilestoneId=@MilestoneId
