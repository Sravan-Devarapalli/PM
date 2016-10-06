﻿CREATE UNIQUE NONCLUSTERED INDEX [UN_TimeEntry]
    ON [dbo].[TimeEntry]
	(PersonId ASC, ChargeCodeId ASC, ChargeCodeDate ASC) WITH ( DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0)
    ON [PRIMARY];
