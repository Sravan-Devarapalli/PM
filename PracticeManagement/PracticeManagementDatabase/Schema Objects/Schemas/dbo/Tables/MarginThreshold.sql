CREATE TABLE [dbo].[MarginThreshold]
(
	[Id] INT NOT NULL IDENTITY (1, 1), 
    [StartDate] DATETIME NOT NULL, 
    [EndDate] DATETIME NOT NULL, 
    [ThresholdVariance] INT NOT NULL
)

GO

CREATE CLUSTERED INDEX [IX_MarginThreshold_Id] ON [dbo].[MarginThreshold] ([Id])

