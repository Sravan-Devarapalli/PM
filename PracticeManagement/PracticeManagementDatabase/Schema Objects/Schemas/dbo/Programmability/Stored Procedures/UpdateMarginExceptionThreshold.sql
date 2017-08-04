CREATE PROCEDURE [dbo].[UpdateMarginExceptionThreshold]
(
	@Id					INT,
	@StartDate			DATETIME,
	@EndDate			DATETIME,
	@ApprovalLevel		INT,
	@MarginGoal			INT,
	@Revenue			DECIMAL(18, 2)
)
AS
BEGIN

	SET NOCOUNT ON;

	UPDATE M
	SET M.StartDate=@StartDate,
		M.EndDate=@EndDate,
		M.ApprovalLevelId=@ApprovalLevel,
		M.MarginGoal=@MarginGoal,
		M.Revenue=@Revenue
	FROM MarginException M
	WHERE M.Id=@Id
		

END
