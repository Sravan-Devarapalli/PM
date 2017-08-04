CREATE PROCEDURE [dbo].[GetMarginExceptionThresholds]
AS

SELECT M.Id,
	   M.StartDate,
	   M.EndDate,
	   M.ApprovalLevelId,
	   l.ApprovalLevel,
	   M.MarginGoal,
	   M.Revenue
FROM MarginException M
JOIN MarginExceptionApprovalLevel L ON M.ApprovalLevelId=L.Id
