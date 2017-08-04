CREATE TABLE [dbo].[MarginExceptionReason]
(
	[Id] INT NOT NULL IDENTITY (1, 1), 
    [Reason] NVARCHAR(100) NOT NULL 
)

GO

CREATE CLUSTERED INDEX [IX_MarginExceptionReason] ON [dbo].[MarginExceptionReason] ([Id])

