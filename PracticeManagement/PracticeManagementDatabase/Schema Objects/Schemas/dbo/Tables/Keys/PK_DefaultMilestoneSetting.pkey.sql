﻿ALTER TABLE dbo.DefaultMilestoneSetting
ADD CONSTRAINT [PK_DefaultMilestoneSetting] PRIMARY KEY NONCLUSTERED (DefaultMilestoneSettingId ASC) WITH (IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);
