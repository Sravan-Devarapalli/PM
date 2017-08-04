CREATE TABLE [dbo].[ClientMarginGoal]
(
	[Id] INT NOT NULL IDENTITY (1, 1), 
    [ClientId] INT NOT NULL, 
    [StartDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NOT NULL, 
    [MarginGoal] INT NOT NULL, 
    [Comments] NVARCHAR(100) NULL
)

GO

CREATE CLUSTERED INDEX [IX_ClientMarginGoal] ON [dbo].[ClientMarginGoal] (Id, ClientId)

