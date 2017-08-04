CREATE TABLE [dbo].[MarginException]
(
	[Id] INT NOT NULL IDENTITY (1, 1), 
    [StartDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NOT NULL, 
    [ApprovalLevelId] INT NOT NULL, 
    [MarginGoal] INT NOT NULL, 
    [Revenue] DECIMAL(18, 2) NOT NULL
)

GO

CREATE CLUSTERED INDEX [IX_MarginException] ON [dbo].[MarginException] ([Id])

