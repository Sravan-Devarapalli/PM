CREATE FUNCTION [dbo].[GetNumberOfCompanyWorkingDays]
(
	@startDate DATETIME,
	@endDate DATETIME
)
RETURNS INT
AS
BEGIN
	DECLARE @res INT

	SELECT @res = COUNT(*) 
	FROM dbo.Calendar
	WHERE Date BETWEEN @startDate AND @endDate AND 
		  DayOff = 0 

	RETURN @res
END

