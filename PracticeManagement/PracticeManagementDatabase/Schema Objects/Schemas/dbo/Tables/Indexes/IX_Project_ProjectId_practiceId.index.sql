﻿CREATE NONCLUSTERED INDEX [IX_Project_ProjectId_practiceId] ON [dbo].[Project] 
(
	[ProjectId] ASC,
	[PracticeId] ASC
)WITH (SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF) ON [PRIMARY]
