CREATE TABLE [dbo].[FixedMilestoneMonthlyRevenue]
(
	[Id]            INT       PRIMARY KEY      IDENTITY (1, 1) NOT NULL, 
    [MilestoneId] INT NOT NULL, 
    [StartDate] DATETIME NULL, 
    [EndDate] DATETIME NULL, 
    [Amount] DECIMAL(18, 2) NULL
)

GO


CREATE NONCLUSTERED INDEX IX_FixedMilestoneMonthlyRevenue_Index1 ON [dbo].[FixedMilestoneMonthlyRevenue] 
(
	[MilestoneId] ASC,
	[StartDate] ASC,
	[EndDate] ASC
)
INCLUDE ( [Amount]) WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]

