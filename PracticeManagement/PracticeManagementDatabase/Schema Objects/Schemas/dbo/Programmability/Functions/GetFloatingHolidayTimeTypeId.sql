CREATE FUNCTION [dbo].[GetFloatingHolidayTimeTypeId]
(
)
RETURNS INT
AS
BEGIN

	RETURN (SELECT tt.TimeTypeId 
			FROM TimeType tt 
			WHERE tt.Code = 'W9610' /* Here 'W9610' is code of Floating Holiday Work Type.*/
			)
END
