CREATE PROCEDURE [dbo].[GetMarginExceptionReasons]
AS
BEGIN

	SELECT M.Id,
		   M.Reason
	FROM MarginExceptionReason M
END 

