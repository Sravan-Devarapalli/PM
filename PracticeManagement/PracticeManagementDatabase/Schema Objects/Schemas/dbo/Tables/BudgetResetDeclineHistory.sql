CREATE TABLE [dbo].[BudgetResetDeclineHistory]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
    [RequestId] INT NOT NULL, 
    [DeclinedBy] INT NULL, 
    [DeclinedDate] DATETIME NULL, 
    [Comments] NVARCHAR(MAX) NULL, 
    CONSTRAINT [FK_BudgetResetDeclineHistory_BudgetResetRequestHistory] FOREIGN KEY ([RequestId]) REFERENCES [BudgetResetRequestHistory]([RequestId])
)

