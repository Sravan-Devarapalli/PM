﻿ALTER TABLE [dbo].[AttributionRecordTypes]
	ADD CONSTRAINT [PK_AttributionRecordTypes]
	PRIMARY KEY CLUSTERED ([AttributionRecordId] ASC) WITH (IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);
