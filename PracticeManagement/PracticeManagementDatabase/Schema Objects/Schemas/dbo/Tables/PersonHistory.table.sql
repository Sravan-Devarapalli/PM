﻿CREATE TABLE [dbo].[PersonHistory]
(
	[Id]			   INT IDENTITY(1,1) NOT NULL,
	[PersonId]         INT            NOT NULL,
	[HireDate]         DATETIME       NULL,
    [TerminationDate]  DATETIME       NULL,
	[RighttoPresentStartDate]         DATETIME       NULL,
    [RighttoPresentEndDate]  DATETIME       NULL,
    [Alias]            NVARCHAR (100) NULL,
    [DefaultPractice]  INT            NULL,
    [FirstName]        NVARCHAR (40)  NOT NULL,
    [LastName]         NVARCHAR (40)  NOT NULL,
    [Notes]            NVARCHAR (MAX) NULL,
    [PersonStatusId]   INT            NOT NULL,
    [EmployeeNumber]   NVARCHAR (12)  NOT NULL,
    [SeniorityId]      INT            NULL,
	[TitleId]          INT            NULL,
    [ManagerId]        INT            NULL,
    [PracticeOwnedId]  INT            NULL,
    [IsDefaultManager] BIT            NOT NULL,
    [TelephoneNumber]  NCHAR (20)     NULL,
	IsWelcomeEmailSent BIT			  NOT NULL,
	IsStrawman		   BIT			  NOT NULL,
	IsOffshore		   BIT			  NOT NULL,
	PaychexID		   NVARCHAR (MAX) NULL,
	DivisionId			INT			  NULL,
	TerminationReasonId	INT			  NULL,
	RecruiterId			INT			  NULL,
	JobSeekerStatusId	INT			  NULL,
	SourceId			INT			  NULL,
	TargetedCompanyId	INT			  NULL,
	EmployeeReferralId	INT			  NULL,
	CohortAssignmentId	INT			  NULL,
	CreatedDate			DATETIME	  NOT NULL,
	CreatedBy			INT			  NULL
	
	CONSTRAINT PK_PersonHistory PRIMARY KEY (Id)
)
GO
CREATE NONCLUSTERED INDEX [IX_PersonHistory]
ON [dbo].[PersonHistory] ([IsStrawman])
INCLUDE ([PersonId],[HireDate],[TerminationDate],[PersonStatusId])
