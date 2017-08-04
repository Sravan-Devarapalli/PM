CREATE PROCEDURE [dbo].[InsertMarginThreshold]
(
	@StartDate			DATETIME,
	@EndDate			DATETIME,
	@ThresholdVariance  INT
)
AS
BEGIN

	SET NOCOUNT ON;
		INSERT INTO dbo.MarginThreshold(StartDate, EndDate,ThresholdVariance)
					Values(@StartDate,@EndDate,@ThresholdVariance)

END
