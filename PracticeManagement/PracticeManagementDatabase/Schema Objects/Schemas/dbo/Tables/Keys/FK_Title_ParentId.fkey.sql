﻿ALTER TABLE [dbo].[Title]
	ADD CONSTRAINT [FK_Title_ParentId] FOREIGN KEY (ParentId)  REFERENCES [dbo].[Title] (TitleId) ON DELETE NO ACTION ON UPDATE NO ACTION;
