CREATE TABLE [dbo].[ProjectBudgetFFMilestoneMonthlyRevenue]
(
	[ProjectId] INT NOT NULL, 
    [MilestoneId] INT NOT NULL, 
    [StartDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NOT NULL, 
    [Amount] DECIMAL(18, 2) NOT NULL 
)

GO

CREATE CLUSTERED INDEX [IX_ProjectBudgetFFMilestoneMonthlyRevenue] ON [dbo].[ProjectBudgetFFMilestoneMonthlyRevenue] (ProjectId,MilestoneId)

