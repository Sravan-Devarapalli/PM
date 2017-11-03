CREATE FUNCTION [dbo].[GetFloatingHolidayTimeTypeIdTable]
(
)
RETURNS TABLE 
AS
	RETURN SELECT tt.TimeTypeId AS FHTimeTypeId
			FROM TimeType tt 
			WHERE tt.Code = 'W9610' --Here 'W9320' is code of Floating Holiday Work Type.
GO

