CREATE PROCEDURE [dbo].[GetMarginExceptionApprovalLevels]
AS

SELECT M.Id,
	   M.ApprovalLevel
FROM MarginExceptionApprovalLevel M

