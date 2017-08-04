CREATE PROCEDURE [dbo].[GetMarginThresholds]

AS

SELECT M.Id,
	   M.StartDate,
	   M.EndDate,
	   M.ThresholdVariance
FROM MarginThreshold M

