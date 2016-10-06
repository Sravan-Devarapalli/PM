﻿CREATE FUNCTION [dbo].[GettingPMTime]
(
	@utcDateTime DateTime 
)
RETURNS DATETIME
AS
BEGIN
	
	DECLARE @resultTime DATETIME
	
	DECLARE @currentTimeZone NVARCHAR(10)
	SET @currentTimeZone = (SELECT Value FROM Settings WHERE SettingsKey = 'TimeZone')
	
	DECLARE @IsDayLightSavingsTimeEffect NVARCHAR(6)
	SET @IsDayLightSavingsTimeEffect = (SELECT Value FROM Settings WHERE SettingsKey = 'IsDayLightSavingsTimeEffect')
		
	IF @currentTimeZone = '-08:00' AND dbo.ISDayLightSavingTime(@utcDateTime) = 1 AND @IsDayLightSavingsTimeEffect = 'true'
	BEGIN
		SET @resultTime = CONVERT(DATETIME, SWITCHOFFSET(CONVERT(DATETIMEOFFSET, @utcDateTime), '-07:00'))
	END
	ELSE
	BEGIN
		SET @resultTime = CONVERT(DATETIME, SWITCHOFFSET(CONVERT(DATETIMEOFFSET, @utcDateTime), @currentTimeZone))
	END
	
	RETURN @resultTime
END
