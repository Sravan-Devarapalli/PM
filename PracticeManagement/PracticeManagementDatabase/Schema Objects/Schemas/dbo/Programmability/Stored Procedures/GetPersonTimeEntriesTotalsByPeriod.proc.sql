-- ====================================================================================================================
-- Author:		Sainath CH
-- Create date: 03-09-2012
-- Description: Gets the persons billable/non billable hours and the billable total(day wise calculated)
-- Updated by : ThulasiRam.P
-- Update Date: 06-05-2012
-- ====================================================================================================================
CREATE PROCEDURE [dbo].[GetPersonTimeEntriesTotalsByPeriod]
(
	@PersonId    INT,
	@StartDate   DATETIME,
	@EndDate     DATETIME
)
AS
BEGIN
	
	DECLARE @StartDateLocal DATETIME,
			@EndDateLocal   DATETIME,
			@PersonIdLocal    INT,
			@HolidayTimeType INT,
			@NOW	DATE,
			@PersonTotalHours INT,
			@FutureDate DATETIME,
			@Yestaday DATE

	SELECT  @StartDateLocal = CONVERT(DATE,@StartDate),
			@EndDateLocal = CONVERT(DATE,@EndDate),
			@PersonIdLocal = @PersonId,
			@HolidayTimeType = dbo.GetHolidayTimeTypeId(),
			@NOW = dbo.GettingPMTime(GETUTCDATE()),
			@FutureDate = dbo.GetFutureDate(),
			@Yestaday = DATEADD(day,-1,@NOW)	

	-- Get person total hours in between the StartDate and EndDate
	--1.Day should not be company holiday and also not converted to substitute day.
	--2.day should be company holiday and it should be taken as a substitute holiday.
	SELECT @PersonTotalHours = (COUNT(PC.Date) * 8) --Estimated working hours per day is 8.
	FROM (
			SELECT CAL.Date,
				   P.PersonId,
				   CAL.DayOff AS CompanyDayOff,
				   PCAL.TimeTypeId,
				   PCAL.SubstituteDate
			  FROM dbo.Calendar AS CAL
				   INNER JOIN dbo.v_PersonHistory AS P ON CAL.Date >= P.HireDate AND CAL.Date <= ISNULL(P.TerminationDate, @FutureDate)
				   LEFT JOIN dbo.PersonCalendar AS PCAL ON PCAL.Date = CAL.Date AND PCAL.PersonId = P.PersonId
		) AS PC 
	WHERE  PC.Date BETWEEN @StartDateLocal AND  @EndDateLocal AND PC.Date < @NOW
			AND DATENAME(weekday,PC.Date) != 'Saturday' AND DATENAME(weekday,PC.Date) != 'Sunday'
			AND PC.PersonId = @PersonIdLocal

	SELECT  ROUND(SUM(CASE WHEN TEH.IsChargeable = 1 THEN TEH.ActualHours 
					 ELSE 0 
				END),2) AS BillableHours,
			ROUND(SUM(CASE WHEN TEH.IsChargeable = 1 AND TE.ChargeCodeDate < @NOW THEN TEH.ActualHours 
					 ELSE 0 
				END),2) AS BillableHoursUntilToday,
			ROUND(SUM(CASE WHEN TEH.IsChargeable = 0 THEN TEH.ActualHours 
					 ELSE 0 
				END),2) AS NonBillableHours,
				@PersonTotalHours AS AvailableHours
	FROM dbo.TimeEntry AS TE 
	INNER JOIN dbo.TimeEntryHours AS TEH  ON TEH.TimeEntryId = TE.TimeEntryId 
	INNER JOIN dbo.ChargeCode CC ON CC.Id = TE.ChargeCodeId 
	INNER JOIN dbo.Client C ON CC.ClientId = C.ClientId
	INNER JOIN dbo.Project PRO ON PRO.ProjectId = CC.ProjectId
	INNER JOIN dbo.PersonStatusHistory PTSH ON TE.ChargeCodeDate BETWEEN PTSH.StartDate  AND ISNULL(PTSH.EndDate,@FutureDate) AND PTSH.PersonId = TE.PersonId 
	WHERE TE.PersonId = @PersonIdLocal 
		AND TE.ChargeCodeDate BETWEEN @StartDateLocal AND @EndDateLocal
		AND (
				CC.timeTypeId != @HolidayTimeType
				OR (CC.timeTypeId = @HolidayTimeType AND PTSH.PersonStatusId IN (1,5) )
			)
END
