﻿ALTER TABLE [dbo].[Pay]
    ADD CONSTRAINT [PK_Pay] PRIMARY KEY CLUSTERED ([Person] ASC, [StartDate] ASC, [EndDate] ASC) WITH (IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);


