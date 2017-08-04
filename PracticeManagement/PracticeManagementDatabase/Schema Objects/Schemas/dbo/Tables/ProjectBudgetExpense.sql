CREATE TABLE [dbo].[ProjectBudgetExpense]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [Name]          NVARCHAR (50)   NOT NULL,
    [Amount]        DECIMAL (18, 2) NOT NULL,
	[Reimbursement] DECIMAL (18, 2) NOT NULL default 0,
    [ProjectId]		INT             NOT NULL,
	[StartDate]		DATETIME		NOT NULL,
	[EndDate]		DATETIME		NOT NULL,
	[MilestoneId]	INT				NULL, 
    [ExpenseTypeId] INT NOT NULL,
)

