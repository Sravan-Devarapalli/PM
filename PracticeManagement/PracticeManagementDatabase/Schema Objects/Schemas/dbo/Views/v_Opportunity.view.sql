﻿CREATE VIEW [dbo].[v_Opportunity]
AS
	SELECT o.OpportunityId,
	       o.Name,	       
	       o.ClientId,
	       o.SalespersonId,
	       o.OpportunityStatusId,
	       OP.Priority,
		   OP.DisplayName,
		   o.PriorityId,
	       o.ProjectedStartDate,
	       o.ProjectedEndDate,
	       o.OpportunityNumber,
	       o.Description,
	       o.PracticeId,
	       o.BuyerName,
	       o.CreateDate,
	       o.Pipeline,
	       o.Proposed,
	       o.SendOut,
           o.ProjectId,
           o.OpportunityIndex,
           o.RevenueType,
		   o.EstimatedRevenue,
		   o.LastUpdated as 'LastUpdate',
		   o.GroupId,
		   o.CloseDate,
		   o.PricingListId,
		   PL.Name AS PricingListName,
		   o.BusinessTypeId,
		   BT.Name AS BusinessTypeName,
		   bg.BusinessGroupId,
		   bg.Name AS BusinessGroupName,
		   g.[Name] as 'GroupName',
	       c.Name AS ClientName,
	       c.DefaultDiscount AS Discount,
	       c.DefaultTerms AS Terms,
	       p.FirstName AS SalespersonFirstName,
	       p.LastName AS SalespersonLastName,
		   p.PreferredFirstName AS SalespersonPreferredFirstName,
		   p.PersonStatusId AS SalespersonStatusId,
	       s.Name AS OpportunityStatusName,
	       r.Name AS PracticeName,
		   own.PersonId AS 'OwnerId',
		   prowner.PersonId AS 'PracticeManagerId',
		   own.PersonStatusId AS 'OwnerStatusId'
	  FROM dbo.Opportunity AS o
	       INNER JOIN dbo.Client AS c ON o.ClientId = c.ClientId
		   INNER JOIN dbo.OpportunityPriorities OP ON OP.Id = O.PriorityId
	       LEFT JOIN dbo.Person AS p ON o.SalespersonId = p.PersonId
		   LEFT JOIN dbo.Person AS own ON o.OwnerID = own.PersonId
	       INNER JOIN dbo.OpportunityStatus AS s ON o.OpportunityStatusId = s.OpportunityStatusId
	       INNER JOIN dbo.Practice AS r ON o.PracticeId = r.PracticeId	
		   LEFT JOIN dbo.ProjectGroup as g on o.GroupId = g.GroupId
		   LEFT JOIN dbo.BusinessGroup as bg on g.BusinessGroupId=bg.BusinessGroupId
		   LEFT JOIN dbo.PricingList AS PL ON o.PricingListId=PL.PricingListId
		   LEFT JOIN dbo.Person as prowner on prowner.PersonId = r.PracticeManagerId
		   LEFT JOIN dbo.BusinessType AS BT ON o.BusinessTypeId=BT.BusinessTypeId

