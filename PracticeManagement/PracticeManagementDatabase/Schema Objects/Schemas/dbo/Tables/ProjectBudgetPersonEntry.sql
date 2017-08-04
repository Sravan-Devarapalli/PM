CREATE TABLE [dbo].[ProjectBudgetPersonEntry]
(
	[ProjectId] INT NOT NULL, 
    [MilestoneId] INT NOT NULL, 
    [PersonId] INT NOT NULL, 
    [StartDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NOT NULL, 
    [Amount] DECIMAL(18, 2) NOT NULL, 
    [HoursPerDay] DECIMAL(8, 2) NOT NULL 
)

GO

CREATE NONCLUSTERED  INDEX [IX_ProjectBudgetPersonEntry] ON [dbo].[ProjectBudgetPersonEntry] 
(ProjectId ASC, MilestoneId ASC, PersonId ASC )
INCLUDE (StartDate, EndDate, HoursPerDay)
GO

CREATE CLUSTERED INDEX [IX_ProjectBudgetPersonEntry_ProjectId] ON [dbo].[ProjectBudgetPersonEntry] (ProjectId, MilestoneId)

