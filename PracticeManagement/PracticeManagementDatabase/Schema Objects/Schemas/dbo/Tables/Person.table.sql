﻿CREATE TABLE [dbo].[Person] (
    [PersonId]			INT     IDENTITY (1, 1) NOT NULL,
    [HireDate]			DATETIME				NOT NULL,
    [TerminationDate]	DATETIME				NULL,
    [Alias]				NVARCHAR (100) 			NULL,
    [DefaultPractice]	INT						NULL,
    [FirstName]			NVARCHAR (40)			NOT NULL,
    [LastName]			NVARCHAR (40)			NOT NULL,
	[PreferredFirstName]	NVARCHAR (40)		NULL,
    [Notes]				NVARCHAR (MAX)			NULL,
    [PersonStatusId]	INT						NOT NULL,
    [EmployeeNumber]	NVARCHAR (12)			NOT NULL,
    [SeniorityId]		INT						NULL,
	[RecruiterId]		INT						NULL,
    [TitleId]			INT						NULL,
    [ManagerId]			INT						NULL,
    [PracticeOwnedId]	INT						NULL,
    [IsDefaultManager]	BIT						NOT NULL,
    [TelephoneNumber]	NCHAR (20)				NULL,
	IsWelcomeEmailSent	BIT						NOT NULL CONSTRAINT DF_Person_IsWelcomeEmailSent DEFAULT(0) ,
	ModifiedDate		DATETIME				NULL,
	IsStrawman			BIT						NOT NULL,
	IsOffshore			BIT						NOT NULL CONSTRAINT DF_Person_IsOffshore DEFAULT(0),
	PaychexID			NVARCHAR (MAX)			NULL,
	DivisionId			INT						NULL,
	TerminationReasonId	INT						NULL,
	PictureData         VARBINARY(MAX)			NULL,
	PictureFileName     NVARCHAR(MAX)			NULL,
	PictureModifiedDate DATETIME				NULL,
	JobSeekerStatusId	INT						NULL,
	SourceId			INT						NULL,
	TargetedCompanyId	INT						NULL,
	EmployeeReferralId	INT						NULL,
	CohortAssignmentId	INT						NULL,
	LocationId			INT						NULL,
	IsMBO				BIT						NOT NULL CONSTRAINT DF_Person_IsMBO DEFAULT(0),
	PracticeLeadershipId	INT					NULL,
	IsInvestmentResource BIT NOT NULL CONSTRAINT DF_Person_IsInvestmentResource DEFAULT(0),
	TargetUtilization	INT						NULL
);

GO
CREATE NONCLUSTERED INDEX [IX_Person_PersonStatusId]
ON [dbo].[Person] ([PersonStatusId])
INCLUDE ([HireDate],[TerminationDate],[Alias],[DefaultPractice],[FirstName],[LastName],[EmployeeNumber],[SeniorityId],[ManagerId],[TelephoneNumber],[PreferredFirstName])
GO

CREATE NONCLUSTERED INDEX [IX_Person_IsStrawman]
ON [dbo].[Person] ([IsStrawman])
INCLUDE ([HireDate],[TerminationDate],[DefaultPractice],[FirstName],[LastName],[PersonStatusId],[IsDefaultManager],[PreferredFirstName])
GO
