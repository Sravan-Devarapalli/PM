CREATE TABLE [dbo].[MarginExceptionApprovalLevel]
(
	[Id] INT NOT NULL IDENTITY (1, 1), 
    [ApprovalLevel] NVARCHAR(12) NOT NULL
)

GO

CREATE CLUSTERED INDEX [IX_MarginExceptionApprovalLevel] ON [dbo].[MarginExceptionApprovalLevel] ([Id])

