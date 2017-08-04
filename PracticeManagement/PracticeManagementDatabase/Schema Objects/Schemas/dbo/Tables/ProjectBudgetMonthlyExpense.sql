CREATE TABLE [dbo].[ProjectBudgetMonthlyExpense]
(
	[Id]            INT       PRIMARY KEY      IDENTITY (1, 1) NOT NULL, 
    [ExpenseId] INT NOT NULL, 
    [StartDate] DATETIME NULL, 
    [EndDate] DATETIME NULL, 
    [Amount] DECIMAL(18, 2) NULL, 
    CONSTRAINT [FK_ProjectBudgetMonthlyExpense_ProjectBudgetExpense] FOREIGN KEY ([ExpenseId]) REFERENCES [dbo].[ProjectBudgetExpense] ([Id]),
)

