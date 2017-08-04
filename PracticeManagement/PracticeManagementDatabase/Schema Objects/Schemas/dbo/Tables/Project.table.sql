﻿CREATE TABLE [dbo].[Project] (
    [ClientId]         INT             NULL,
    [ProjectId]        INT             IDENTITY (1, 1) NOT NULL,
    [Discount]         DECIMAL (18, 2) NOT NULL,
    [Terms]            INT             NOT NULL,
    [Name]             NVARCHAR (100)  NOT NULL,
    [PracticeId]       INT             NOT NULL,
    [StartDate]        DATETIME        NULL,
    [EndDate]          DATETIME        NULL,
    [ProjectStatusId]  INT             NOT NULL,
    [ProjectNumber]    NVARCHAR (12)   NOT NULL,
    [BuyerName]        NVARCHAR (100)  NULL,
    [OpportunityId]    INT             NULL,
    [GroupId]          INT             NULL,
    [IsChargeable]     BIT             NULL,
	[ExecutiveInChargeId]	   INT	   NULL,
	[Description]      NVARCHAR (MAX)  NULL,
	[CanCreateCustomWorkTypes]	BIT	   NOT NULL,
	[IsInternal]	            BIT	   NOT NULL CONSTRAINT DF_Project_IsInternal DEFAULT 0,--added as internal client can be having external projects.
	[IsAllowedToShow]			BIT	   NOT NULL CONSTRAINT DF_Project_IsAllowedToShow DEFAULT 1,--For not showing internal projects(like PTO,HOL,etc)/"Business Development" project in overall PM site.
	[IsAdministrative]          BIT    NOT NULL CONSTRAINT [DF_Project_IsAdministrative] DEFAULT 0,
	[IsNoteRequired]            BIT    NOT NULL CONSTRAINT DF_Project_IsNoteRequired DEFAULT (1),
	[ProjectManagerId]            INT NULL,
	[SowBudget]					DECIMAL(18,2) NULL,
	[POAmount]					DECIMAL(18,2) NULL,
	[PricingListId]				INT NULL,
	[BusinessTypeId]			INT NULL,
	[ReviewerId]				INT NULL,
	EngagementManagerId			INT NULL,
	IsSeniorManagerUnassigned	BIT NOT NULL CONSTRAINT DF_Project_IsSeniorManagerUnassigned DEFAULT 0,
	[PONumber]					NVARCHAR(100)  NULL,
	SalesPersonId				INT				NULL,
	[InvisibleInTimeEntry]		BIT NULL,
	[IsBusinessDevelopment]		BIT		NOT NULL CONSTRAINT DF_Project_IsBusinessDevelopment DEFAULT 0, -- to indicate the projects responsible for Business Development hours
	[CreatedDate]				DATETIME NULL,
	[DivisionId]				INT  	NOT NULL,
	[ChannelId]					INT		NOT NULL CONSTRAINT DF_Project_ChannelId DEFAULT 0,
	[SubChannel]				NVARCHAR(100)  NULL CONSTRAINT DF_Project_SubChannel DEFAULT NULL,
	[RevenueTypeId]				INT		NOT NULL CONSTRAINT DF_Project_RevenueTypeId DEFAULT 0,
	[OfferingId]				INT		NOT NULL CONSTRAINT DF_Project_OfferingId DEFAULT 0,
	[IsClientTimeEntryRequired]   BIT    NOT NULL CONSTRAINT DF_Project_IsClientTimeEntryRequired DEFAULT (0),
	[PreviousProjectNumber]    NVARCHAR (12)   NULL,
	[OutsourceId]				INT		NOT NULL CONSTRAINT DF_Project_OutsourceId DEFAULT (3) --NotApplicable
    FOREIGN KEY ([GroupId]) REFERENCES [dbo].[ProjectGroup] ([GroupId]) ON DELETE NO ACTION ON UPDATE NO ACTION, 
    [Budget] INT NULL, 
    [ExceptionMargin] DECIMAL(5, 2) NULL, 
    [BudgetSetDate] DATETIME NULL, 
    [ExceptionRevenue] DECIMAL(18, 2) NULL
);
GO

CREATE NONCLUSTERED INDEX [IX_Project_GroupIdSalesPersonId]
ON [dbo].[Project] ([GroupId],[SalesPersonId])
GO

CREATE NONCLUSTERED INDEX [IX_Project_IsAllowedToShow]
ON [dbo].[Project] ([IsAllowedToShow])
INCLUDE ([StartDate],[EndDate],[ProjectStatusId],[ExecutiveInChargeId],[ProjectManagerId],[EngagementManagerId])
GO
