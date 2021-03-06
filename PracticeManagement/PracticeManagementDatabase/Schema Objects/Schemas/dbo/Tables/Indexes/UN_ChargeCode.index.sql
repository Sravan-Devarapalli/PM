﻿CREATE UNIQUE NONCLUSTERED INDEX [UN_ChargeCode]
    ON [dbo].[ChargeCode]
	(ClientId ASC, ProjectGroupId ASC, ProjectId ASC, PhaseId ASC, TimeTypeId ASC) WITH ( DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF, ONLINE = OFF, MAXDOP = 0)
    ON [PRIMARY];
