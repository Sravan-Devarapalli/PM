﻿-- =============================================
-- Author:		Nikita Gonhcarenko
-- Create date: 2009-12-08
-- Description:	Get time entries for the person
-- =============================================
CREATE PROCEDURE [dbo].[PersonTimeEntries] 
	@PersonId INT,
	@StartDate DATETIME,
	@EndDate DATETIME	
AS
BEGIN
	SET NOCOUNT ON;
	
SELECT te.*, cal.CompanyDayOff, cal.DayOff, cal.Date, CAST(0 as bit) as 'ReadOnly', tt.IsAllowedToEdit
  FROM v_TimeEntries AS te
  left join v_PersonCalendar as cal on te.PersonId = cal.PersonId and te.MilestoneDate = cal.Date
  LEFT JOIN TimeType tt ON tt.TimeTypeId = te.TimeTypeId
  WHERE 
	te.MilestoneDate BETWEEN @StartDate AND @EndDate
	AND te.PersonId = @PersonId  
END

