CREATE PROCEDURE [dbo].[DeleteFixedMilestoneMonthlyRevenues]
(
	@MilestoneId	INT
)
AS
	SET NOCOUNT ON

	DELETE FROM FixedMilestoneMonthlyRevenue
	WHERE MilestoneId=@MilestoneId
