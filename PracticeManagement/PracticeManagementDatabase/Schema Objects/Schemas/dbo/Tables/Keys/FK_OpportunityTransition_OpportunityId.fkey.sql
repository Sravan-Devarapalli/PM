ALTER TABLE [dbo].[OpportunityTransition]
    ADD CONSTRAINT [FK_OpportunityTransition_OpportunityId] FOREIGN KEY ([OpportunityId]) REFERENCES [dbo].[Opportunity] ([OpportunityId]) ON DELETE CASCADE ON UPDATE NO ACTION;


