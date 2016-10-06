﻿CREATE TABLE [dbo].[Title]
(
	TitleId			INT	IDENTITY (1, 1) NOT NULL,
	Title			NVARCHAR(100) NOT NULL,
	TitleTypeId		INT NOT NULL,
	SortOrder		INT NOT NULL CONSTRAINT DF_Title_SortOrder DEFAULT(0),
	PTOAccrual		INT NOT NULL CONSTRAINT DF_Title_PTOAccrual DEFAULT(0),
	MinimumSalary	INT NULL,
	MaximumSalary	INT NULL,
	ParentId		INT	NULL,
	PositionId		INT NULL,
	Active			BIT NULL DEFAULT 1,
	ShowInMeetingReport	BIT NOT NULL CONSTRAINT DF_Title_ShowInMeetingReport DEFAULT(0)
	CONSTRAINT PK_Title_TitleId     PRIMARY KEY CLUSTERED(TitleId),
	CONSTRAINT [UQ_Title_Title] UNIQUE NONCLUSTERED(Title)
)

