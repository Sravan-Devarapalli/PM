CREATE PROCEDURE [dbo].[UpdateMarginThreshold]
(
	@Id					INT,
	@StartDate			DATETIME,
	@EndDate			DATETIME,
	@ThresholdVariance  INT
)
AS
BEGIN

	SET NOCOUNT ON;

	UPDATE M
	SET M.StartDate=@StartDate,
		M.EndDate=@EndDate,
		M.ThresholdVariance=@ThresholdVariance
	FROM MarginThreshold M
	WHERE M.Id=@Id
	
END

