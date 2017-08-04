CREATE PROCEDURE [dbo].[GetClientMarginColorInfo]
(
	@ClientId INT,
	@StartDate DATETIME,
	@EndDate   DATETIME,
	@ProjectId INT
)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ClientIdLocal INT,
			@MarginGoal    DECIMAL(6,2) = -1,
			@MarginThreshold INT =-1,
			@StartDateLocal DATETIME = @StartDate,
			@EndDateLocal  DATETIME =@EndDate

	SELECT @ClientIdLocal=@ClientId

	SELECT @MarginGoal= ExceptionMargin
	from project where projectid =@ProjectId and ExceptionMargin is not null

	if(@MarginGoal=-1)
	BEGIN
		SELECT @MarginGoal=c.MarginGoal
		FROM ClientMarginGoal C
		WHERE C.ClientId=@ClientIdLocal AND (C.StartDate<= @StartDateLocal AND C.EndDate>=@StartDateLocal)
	END

	SELECT @MarginThreshold=M.ThresholdVariance
	FROM MarginThreshold M
	WHERE M.StartDate<=@StartDateLocal AND M.EndDate>=@StartDateLocal

	if(@MarginGoal=-1)
	BEGIN
		SELECT @MarginGoal=50
	END

	if(@MarginThreshold=-1)
	BEGIN
		SELECT  @MarginThreshold=5
	END

	DECLARE @MarginColor TABLE
	(
		ColorId INT,
		StartRange DECIMAL(6,2),
		EndRange  DECIMAL(6,2)
	)
	
	INSERT INTO @MarginColor(ColorId,StartRange,EndRange) VALUES(2, 0, @MarginGoal-@MarginThreshold-0.01),(3, @MarginGoal-@MarginThreshold,@MarginGoal-0.01),(1,@MarginGoal,150)

	SELECT cm.[ColorId]
		  ,cm.[StartRange]
		  ,cm.[EndRange]
		  ,color.Value
		  ,color.Description
	  FROM @MarginColor AS cm 
	  INNER JOIN dbo.Color color ON cm.[ColorId] = color.Id
	  ORDER BY cm.StartRange
END
GO
