﻿ALTER TABLE [dbo].[PersonCalendar]
    ADD CONSTRAINT [FK_PersonCalendar_ApprovedBy] FOREIGN KEY (ApprovedBy) REFERENCES [dbo].[Person] ([PersonId]) ON DELETE NO ACTION ON UPDATE NO ACTION;


