﻿ALTER TABLE [dbo].[ErrorMessage]
    ADD CONSTRAINT [PK_ErrorMessage] PRIMARY KEY CLUSTERED ([MessageId] ASC)
    WITH (IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);

