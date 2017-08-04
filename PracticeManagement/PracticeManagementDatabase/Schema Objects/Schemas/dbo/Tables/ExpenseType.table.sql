CREATE TABLE [dbo].[ExpenseType]
(
	[Id]            INT             IDENTITY (1, 1) NOT NULL,
    [Name]          NVARCHAR (100)   NOT NULL,
)

GO

CREATE CLUSTERED INDEX  [IX_ExpenseType_Id] ON [dbo].[ExpenseType] ([Id])

