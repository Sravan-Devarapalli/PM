﻿ALTER TABLE [dbo].[OverrideExceptionHistory]
	ADD CONSTRAINT [FK_OverrideException_PersonId] 
	FOREIGN KEY (PersonId)
	REFERENCES dbo.Person (PersonId) ON DELETE NO ACTION ON UPDATE NO ACTION;	


