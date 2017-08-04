CREATE PROCEDURE [dbo].[FinancialsGetByProjectIds]
(
	@ProjectId   NVARCHAR(MAX)
)
AS
SET NOCOUNT ON;
DECLARE @ProjectIdLocal	NVARCHAR(MAX),
@MaxProjectId   NVARCHAR(MAX),
@MinProjectId   NVARCHAR(MAX)

SELECT @ProjectIdLocal = @ProjectId

DECLARE @ProjectIDs TABLE
(
		ResultId INT
)

INSERT INTO @ProjectIDs
SELECT * FROM dbo.ConvertStringListIntoTable(@ProjectIdLocal)
SELECT @MaxProjectId = MAX(ResultId), @MinProjectId = MIN(ResultId) FROM  @ProjectIDs

	
SELECT A.ProjectId,
SUM(A.PersonMilestoneDailyAmount) AS Revenue,
SUM(A.PersonMilestoneDailyAmount - A.PersonDiscountDailyAmount) AS RevenueNet,
SUM(A.PersonMilestoneDailyAmount - A.PersonDiscountDailyAmount -
(CASE WHEN A.SLHR  >= A.PayRate + A.MLFOverheadRate THEN A.SLHR 
ELSE A.PayRate + A.MLFOverheadRate END) *ISNULL(A.PersonHoursPerDay, 0)) GrossMargin
INTO #tempFinRet 
FROM
(
SELECT f.ProjectId,
f.PersonMilestoneDailyAmount,
f.PersonDiscountDailyAmount,
(ISNULL(f.PayRate, 0) + ISNULL(f.OverheadRate, 0)+ISNULL(f.BonusRate,0)+ISNULL(f.VacationRate,0)) SLHR,
ISNULL(f.PayRate, 0) PayRate,
f.MLFOverheadRate,
f.PersonHoursPerDay
FROM v_FinancialsRetrospective f
WHERE f.ProjectId BETWEEN @MinProjectId AND @MaxProjectId
				
)A
GROUP BY A.ProjectId 
HAVING A.ProjectId in (SELECT * FROM @ProjectIDs)
	
CREATE CLUSTERED INDEX CIX_FinRet on #tempFinRet(ProjectId)



SELECT
P.ProjectId,
ISNULL(pf.Revenue,0)  as 'Revenue',
ISNULL(pf.RevenueNet,0)+ISNULL(PE.ReimbursedExpenseSum,0) as 'RevenueNet',
ISNULL(pf.GrossMargin,0)+((ISNULL(PE.ReimbursedExpenseSum,0)-ISNULL(PE.ExpenseSum,0)))  as 'GrossMargin'
FROM  Project p
INNER JOIN @ProjectIDs Pids ON Pids.ResultId = p.ProjectId
LEFT JOIN #tempFinRet pf ON p.ProjectId = pf.ProjectId
LEFT JOIN v_ProjectTotalExpenses PE ON P.ProjectId = PE.ProjectId
WHERE (pf.ProjectId IS NOT NULL OR PE.ProjectId IS NOT NULL)
AND P.StartDate IS NOT NULL AND P.EndDate IS NOT NULL	
	
GO



