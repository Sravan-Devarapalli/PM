﻿CREATE TABLE [dbo].[Announcements]
(
	Id			INT IDENTITY(1,1),
	[Date]		DATETIME NOT NULL,
	[Text]		NVARCHAR(MAX) NULL,
	RichText	NVARCHAR(MAX) NOT NULL,
	 CONSTRAINT PK_Announcements_Id PRIMARY KEY CLUSTERED(Id)
)
