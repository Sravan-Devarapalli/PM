CREATE TABLE [dbo].[BudgetResetRequestHistory]
(
	[RequestId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
    [ProjectId] INT NULL, 
    [RequestedBy] INT NULL, 
    [RequestDate] DATETIME NULL,
	[Comments] NVARCHAR(MAX) NOT NULL, 
    [ResetType] INT NOT NULL, 
    [BudgetToDate] DATETIME NULL
    
)




