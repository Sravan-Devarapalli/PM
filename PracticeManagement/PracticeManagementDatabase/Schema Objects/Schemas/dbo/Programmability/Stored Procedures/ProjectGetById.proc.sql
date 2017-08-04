﻿-- =============================================
-- Description:	Get project Details.
-- Updated By:	ThulasiRam.P
-- Updated Date: 2012-06-07
-- =============================================
CREATE PROCEDURE  [dbo].[ProjectGetById]
(
	@ProjectId	         INT
)
AS
	SET NOCOUNT ON

	declare @IsReset BIT =0,
			@TierOneExceptionStatus INT,
			@TierTwoExceptionStatus INT,
			@MarginExceptionId INT,
			@MarginRequestor  INT,
			@IsCOMilestoneExists BIT = 0

	 IF EXISTS(SELECT 1   FROM dbo.BudgetResetRequestHistory BRH
		  LEFT JOIN dbo.BudgetResetApprovalHistory BAH on BRH.requestId=BAH.requestid 
		  WHERE BRH.ProjectId=@ProjectId  AND BAH.Id IS NOT NULL)
	 BEGIN 
		  SET @IsReset=1
	 END
	 ELSE
	 BEGIN
		  SET @IsReset=0
	 END

	 SELECT TOP 1 @MarginExceptionId=Id, @TierOneExceptionStatus = TierOneStatus, @TierTwoExceptionStatus=TierTwoStatus, @MarginRequestor=Requestor  from MarginExceptionRequest 
	 WHERE projectid=@ProjectId
	 ORDER BY 1 DESC

	 IF EXISTS(SELECT 1
				FROM Milestone M 
				LEFT JOIN ProjectBudgetHistory PBH on PBH.MilestoneId = M.MilestoneId and PBH.MilestoneId IS NOT NULL AND PBH.IsActive = 1
				WHERE M.ProjectId = @ProjectId  AND PBH.MilestoneId is  NULL AND M.MilestoneType = 2 )
	BEGIN
		SET @IsCOMilestoneExists  = 1
	END
	
	SELECT person.LastName+', '+person.FirstName AS PracticeOwnerName,
		   p.ClientId,
		   p.IsMarginColorInfoEnabled,
	       p.ProjectId,
	       p.Discount,
	       p.Terms,
	       p.Name,
	       p.PracticeManagerId,
	       p.PracticeId,
	       p.StartDate,
	       p.EndDate,
	       p.ClientName,
	       p.PracticeName,
	       p.ProjectStatusId,
	       p.ProjectStatusName,
		   p.ProjectNumber,
	       p.BuyerName,
	       p.OpportunityId,
		   O.OpportunityNumber,
	       p.GroupId,
		   p.PricingListId,
		   pl.Name AS PricingListName,
		   p.BusinessTypeId,
	       p.ProjectIsChargeable,
	       p.ClientIsChargeable,
		   p.ProjectManagersIdFirstNameLastName,
		   p.ExecutiveInChargeId AS DirectorId,
		   p.DirectorLastName,
		   p.DirectorFirstName,
		   pg.Name AS GroupName,
		   p.Description,
		   1 InUse,
		   CASE WHEN A.ProjectId IS NOT NULL THEN 1 
					ELSE 0 END AS HasAttachments,
		   p.CanCreateCustomWorkTypes,
		   p.IsInternal,
		   p.ClientIsInternal,
		   p.ProjectManagerId AS ProjectOwnerId,
		   CASE (SELECT COUNT(*) 
				FROM dbo.ChargeCode CC 
				INNER JOIN TimeEntry TE ON TE.ChargeCodeId = CC.Id AND CC.ProjectId = p.ProjectId) 
			WHEN 0 THEN CAST(0 AS BIT)
			ELSE CAST(1 AS BIT) END AS [HasTimeEntries],
			p.IsNoteRequired,
			p.SowBudget,
			p.POAmount,
			p.ClientIsNoteRequired,
			p.ProjectCapabilityIds,
			CASE WHEN p.IsSeniorManagerUnassigned = 1 THEN -1 ELSE  sm.PersonId  END AS 'SeniorManagerId',
			CASE WHEN p.IsSeniorManagerUnassigned = 1 THEN 'Unassigned' ELSE  sm.LastName+', ' +sm.FirstName END AS 'SeniorManagerName',
			re.PersonId AS 'ReviewerId',
			re.LastName+', ' +re.FirstName AS 'ReviewerName',
			p.PONumber,
			p.SalesPersonId,
			dbo.GetProjectManagersAliasesList(p.ProjectId) AS ToAddressList,
			owner.Alias AS ProjectOwnerAlias,
			p.DivisionId,
			p.ChannelId,
			p.SubChannel,
			p.RevenueTypeId,
			p.OfferingId,
			p.IsClientTimeEntryRequired,
			PrevProject.ProjectId AS PreviousProjectId,
			PrevProject.ProjectNumber AS PreviousProjectNumber,
			p.OutsourceId,
			p.Budget,
			p.IsBudgetResetPending,
			p.BudgetResetRequestId,
			p.EnableBudgetRequest,
			@IsReset as IsReset,
			p.ExceptionMargin,
			p.ExceptionRevenue,
			@TierOneExceptionStatus as TierOneExceptionStatus,
			@TierTwoExceptionStatus as TierTwoExceptionStatus,
			@MarginExceptionId as MarginExceptionId,
			@MarginRequestor as MarginRequestorId,
			p.requestorName,
			@IsCOMilestoneExists as IsCOMilestoneExists,
			p.BudgetToDate
	  FROM dbo.v_Project AS p
	  INNER JOIN dbo.ProjectGroup AS pg ON p.GroupId = pg.GroupId
	  LEFT JOIN dbo.Opportunity AS O ON O.OpportunityId = P.OpportunityId
	  LEFT JOIN dbo.Person AS person ON p.PracticeManagerId = person.PersonId
	  LEFT JOIN dbo.Person AS sm ON p.EngagementManagerId = sm.PersonId
	  LEFT JOIN dbo.Person owner ON owner.PersonId = p.ProjectManagerId
	  LEFT JOIN dbo.Person AS re ON p.ReviewerId = re.PersonId
	  LEFT JOIN dbo.PricingList AS pl ON pl.PricingListId = p.PricingListId
	  LEFT JOIN dbo.Project AS PrevProject ON p.PreviousProjectNumber=PrevProject.ProjectNumber
	  OUTER APPLY (SELECT TOP 1 ProjectId FROM ProjectAttachment as pa WHERE pa.ProjectId = p.ProjectId) A
	  WHERE p.ProjectId = @ProjectId
	  

