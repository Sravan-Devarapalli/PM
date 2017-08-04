CREATE TABLE [dbo].[MilestonePersonHistory]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY (1, 1),
    [ProjectId] INT NOT NULL, 
    [MilestoneId] INT NOT NULL, 
    [MilestonePersonId] INT NOT NULL, 
    [OldStartDate] DATETIME NULL, 
    [NewStartDate] DATETIME NULL, 
    [OldEndDate] DATETIME NULL, 
    [NewEndDate] DATETIME NULL, 
    [OldHoursPerDay] DECIMAL(4, 2) NULL, 
    [NewHoursPerDay] DECIMAL(4, 2) NULL, 
    [OldAmount] DECIMAL(18, 2) NULL, 
    [NewAmount] DECIMAL(18, 2) NULL, 
    [LogTime] DATETIME NULL, 
    [UpdatedBy] INT NULL, 
    [PersonId] INT NULL, 
    [MilestonePersonEntryId] INT NULL, 
    [RoleId] INT NULL
)

GO

CREATE NONCLUSTERED INDEX [NCIX_MilestonePersonHistory_ProjectId_LogTime] ON [dbo].[MilestonePersonHistory]
(
	[ProjectId] ASC,
	[LogTime] ASC
)
INCLUDE ( 	[MilestoneId],
	[MilestonePersonId],
	[NewStartDate],
	[NewEndDate],
	[NewHoursPerDay],
	[NewAmount],
	[PersonId],
	[MilestonePersonEntryId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
GO

