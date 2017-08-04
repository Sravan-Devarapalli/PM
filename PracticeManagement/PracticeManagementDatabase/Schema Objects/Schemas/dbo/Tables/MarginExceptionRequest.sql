CREATE TABLE [dbo].[MarginExceptionRequest]
(
	[Id] INT IDENTITY (1, 1) NOT NULL, 
    [ProjectId] INT NOT NULL, 
    [Requestor] INT NOT NULL, 
    [Approver] INT NOT NULL, 
    [TargetMargin] DECIMAL(5, 2) NOT NULL, 
    [RequestDate] DATETIME NOT NULL, 
    [ApprovedBy] INT NULL, 
    [TierOneStatus] INT NOT NULL DEFAULT 0, 
	[TierTwoStatus] INT NOT NULL DEFAULT 0,
    [ResponseDate] DATETIME NULL, 
    [TargetRevenue] DECIMAL(18, 2) NULL, 
    [Comments] NVARCHAR(MAX) NULL, 
    [IsRevenueException] BIT NOT NULL DEFAULT 0
)

GO

CREATE CLUSTERED INDEX [IX_MarginExceptionRequest] ON [dbo].[MarginExceptionRequest] (Id, ProjectId)

