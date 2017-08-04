CREATE TABLE [dbo].[ProjectBudgetHistory]
(
	[ProjectId] INT NOT NULL , 
    [Revenue] DECIMAL(18, 2) NULL, 
    [Expense] DECIMAL(18, 2) NULL, 
    [ReimbursedExpense] DECIMAL(18, 2) NULL, 
    [Discount] DECIMAL(5, 2) NULL, 
    [COGS] DECIMAL(18, 2) NULL, 
    [IsActive] BIT NOT NULL, 
    [LogTime] DATETIME NULL, 
    [UpdatedBy] INT NULL, 
    [MilestoneId] INT NULL DEFAULT Null, 
    [IsHourlyAmount] INT NULL DEFAULT NULL, 
    CONSTRAINT [FK_ProjectBudgetHistory_Project] FOREIGN KEY ([ProjectId]) REFERENCES [Project]([ProjectId])
)

GO

CREATE CLUSTERED INDEX [IX_ProjectBudgetHistory_ProjectId] ON [dbo].[ProjectBudgetHistory] ([ProjectId])

