CREATE TABLE [dbo].[Attribution]
(
	[AttributionId]				INT	IDENTITY (1, 1)	NOT NULL,
	[ProjectId]					INT					NOT NULL,
	[AttributionTypeId]			INT					NOT NULL,
	[AttributionRecordTypeId]	INT					NOT NULL,
	[TargetId]					INT					NOT NULL,
	[StartDate]					DATETIME			NULL,
	[EndDate]					DATETIME			NULL,
	[Percentage]				DECIMAL(5,2)		NULL
)
GO

CREATE NONCLUSTERED INDEX [IX_Attribution_ProjectId]
ON [dbo].[Attribution] ([ProjectId])
GO
