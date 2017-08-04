CREATE PROCEDURE [dbo].[GetMilestoneBudgetFinancials]
(
	@MilestoneId      INT
)
AS
BEGIN
	SET NOCOUNT ON;

	Select 1 AS FinanceType,
		   PBH.ProjectId,
		   PBH.MilestoneId,
		   ISNULL(PBH.Revenue,0)  as 'Revenue',
		   ISNULL(PBH.Revenue,0)-ISNULL((ISNULL(PBH.Revenue,0)*ISNULL(PBH.Discount,0))/100,0) AS 'RevenueNet',
		   ISNULL(PBH.COGS,0)  AS 'Cogs',
		   (ISNULL(PBH.Revenue,0)+ISNULL(PBH.ReimbursedExpense,0)-ISNULL(PBH.Expense,0) -ISNULL((ISNULL(PBH.Revenue,0)*ISNULL(PBH.Discount,0))/100,0)-PBH.COGS) as 'GrossMargin',
		   PBH.Expense,
		   PBH.ReimbursedExpense
	FROM ProjectBudgetHistory PBH
	WHERE PBH.IsActive=1 AND PBH.MilestoneId = @MilestoneId

END

