﻿ALTER TABLE [dbo].[TimeEntryHours]
ADD CONSTRAINT [UN_TimeEntryHours] UNIQUE NONCLUSTERED ([TimeentryId] ASC, [IsChargeable] ASC) WITH (IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF) ON [PRIMARY];


