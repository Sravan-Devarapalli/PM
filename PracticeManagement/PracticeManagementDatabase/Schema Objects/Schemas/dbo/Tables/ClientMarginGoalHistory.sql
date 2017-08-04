CREATE TABLE [dbo].[ClientMarginGoalHistory]
(
	[Id] INT NOT NULL IDENTITY (1, 1), 
    [ClientId] INT NOT NULL, 
    [Activity] INT NOT NULL, 
    [OldStartDate] DATETIME NULL, 
    [NewStartDate] DATETIME NULL, 
    [OldEndDate] DATETIME NULL, 
    [NewEndDate] DATETIME NULL, 
    [OldMarginGoal] INT NULL, 
    [NewMarginGoal] INT NULL, 
    [Comments] NVARCHAR(100) NULL, 
    [PersonId] INT NOT NULL, 
    [LogTime] DATETIME NULL
)

GO

CREATE CLUSTERED INDEX [IX_ClientMarginGoalHistory] ON [dbo].[ClientMarginGoalHistory] (Id,ClientId)

