CREATE PROCEDURE [dbo].[GetMarginExceptionThresholdsForPeriod]
(
	@StartDate	DATETIME,
	@EndDate	DATETIME,
	@ClientId	INT
)
AS
BEGIN
	
	DECLARE @ClientIdLocal INT,
			@MarginGoal    INT = -1,
			@StartDateLocal DATETIME = @StartDate,
			@EndDateLocal  DATETIME =@EndDate

	SELECT @ClientIdLocal=@ClientId

	SELECT @MarginGoal=c.MarginGoal
	FROM ClientMarginGoal C
	WHERE C.ClientId=@ClientIdLocal AND (C.StartDate<= @StartDateLocal AND C.EndDate>=@StartDateLocal)

	if(@MarginGoal=-1)
	BEGIN
		SELECT @MarginGoal=50 -- use set insted of Select 
	END

	SELECT ME.Id,
		   ME.StartDate,
		   ME.EndDate,
		   ME.ApprovalLevelId,
		   ME.MarginGoal AS 'MarginThreshold',
		   ME.Revenue,
		   @MarginGoal as 'MarginGoal'
	FROM MarginException ME
	WHERE ME.StartDate<=@StartDateLocal AND ME.EndDate>=@StartDateLocal

END 
