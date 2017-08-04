CREATE TABLE [dbo].[MilestonePerson] (
    [MilestoneId]       INT NOT NULL,
    [PersonId]          INT NOT NULL,
    [MilestonePersonId] INT IDENTITY (1, 1) NOT NULL
);

GO
CREATE NONCLUSTERED INDEX [IX_MilestonePerson_PersonId]
ON [dbo].[MilestonePerson] ([PersonId])
