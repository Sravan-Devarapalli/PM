CREATE PROCEDURE [dbo].[InsertMarginExceptionThreshold]
(
	@StartDate			DATETIME,
	@EndDate			DATETIME,
	@ApprovalLevel		INT,
	@MarginGoal			INT,
	@Revenue			DECIMAL(18, 2)
)
AS
BEGIN

	SET NOCOUNT ON;
		INSERT INTO dbo.MarginException(StartDate,EndDate,ApprovalLevelId,MarginGoal,Revenue)
					Values(@StartDate,@EndDate,@ApprovalLevel,@MarginGoal,@Revenue)

END
