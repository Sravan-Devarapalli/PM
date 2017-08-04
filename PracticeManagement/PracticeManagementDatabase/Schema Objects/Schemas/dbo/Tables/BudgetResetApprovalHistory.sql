CREATE TABLE [dbo].[BudgetResetApprovalHistory]
(
	[Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
    [RequestId] INT NOT NULL, 
    [ApprovedBy] INT NULL, 
    [ApprovedDate] DATETIME NULL, 
    [ResetType] INT NOT NULL, 
    [BudgetToDate] DATETIME NULL, 
    CONSTRAINT [FK_BudgetResetApprovalHostory_BudgetResetRequestHistory] FOREIGN KEY ([RequestId]) REFERENCES [BudgetResetRequestHistory]([RequestId])
)

