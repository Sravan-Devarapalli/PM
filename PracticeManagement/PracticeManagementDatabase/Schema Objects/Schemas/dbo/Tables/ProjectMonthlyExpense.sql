CREATE TABLE [dbo].[ProjectMonthlyExpense]
(
	[Id]            INT       PRIMARY KEY      IDENTITY (1, 1) NOT NULL, 
    [ExpenseId] INT NOT NULL, 
    [StartDate] DATETIME NULL, 
    [EndDate] DATETIME NULL, 
    [EstimatedAmount] DECIMAL(18, 2) NULL, 
    [ActualAmount] DECIMAL(18, 2) NULL,
)

