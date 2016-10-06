﻿CREATE VIEW [dbo].[v_ConsultingDemand]
	AS 
	SELECT	CAL.MonthStartDate,
			C.ClientId,
			C.Name AS AccountName,
			P.ProjectId,
			P.ProjectNumber,
			P.Name AS ProjectName,
			P.Description AS ProjectDescription,
			O.OpportunityId,
			O.OpportunityNumber,
			PS.Name AS SalesStage,
			Per.Personid,
			Per.FirstName AS Skill,
			Per.LastName AS Title,
			CASE WHEN MPE.StartDate < CAL.MonthStartDate THEN CAL.MonthStartDate ELSE MPE.StartDate END ResourceStartDate,
			COUNT(DISTINCT MPE.Id) as [Count]
	FROM dbo.Person Per
	INNER JOIN dbo.MilestonePerson MP ON MP.PersonId =Per.PersonId 
									AND Per.IsStrawman=1
	INNER JOIN dbo.Milestone M ON MP.MilestoneId=M.MilestoneId
	INNER JOIN Project P ON M.ProjectId = P.ProjectId AND (P.ProjectStatusId=3 OR P.ProjectStatusId=2)
	INNER JOIN ProjectStatus PS ON PS.ProjectStatusId = P.ProjectStatusId
	INNER JOIN Client C ON P.ClientId=C.ClientId
	INNER JOIN MilestonePersonEntry MPE ON MPE.MilestonePersonId=MP.MilestonePersonId 
	INNER JOIN dbo.Calendar CAL ON MPE.StartDate<=cal.MonthEndDate 
								AND cal.MonthStartDate <= MPE.EndDate
	LEFT JOIN Opportunity O ON O.ProjectId=P.ProjectId
	GROUP BY CAL.MonthStartDate,
			 C.ClientId,
			 C.Name,
			 P.ProjectId,
			 P.Name,
			 P.Description,
			 P.ProjectNumber,
			 O.OpportunityId,
			 O.OpportunityNumber,
			 PS.Name,
			 Per.PersonId,
			 Per.FirstName,
			 Per.LastName,
			 MPE.StartDate
	UNION ALL 
	SELECT CAL.MonthStartDate,
		   C.ClientId,
		   C.Name AS AccountName,
		   -1 AS ProjectId,
		   '' AS ProjectNumber,
		   O.Name AS ProjectName,
		   O.Description As ProjectDescription,
		   O.OpportunityId,
		   O.OpportunityNumber,
		   OPro.DisplayName AS SalesStage,
		   Per.PersonId,
		   Per.FirstName,
		   Per.LastName,
		   CASE WHEN OP.NeedBy<cal.MonthStartDate THEN cal.MonthStartDate ELSE OP.NeedBy END ResourceStartDate,
		   SUM(OP.Quantity)/MAX(CAl.DaysInMonth) as [Count]
	FROM dbo.Opportunity O 
	INNER JOIN Client C ON O.ClientId=C.ClientId AND O.ProjectId IS NULL 
	INNER JOIN dbo.OpportunityPriorities OPro ON OPro.Id = O.PriorityId AND (OPro.Priority='A' OR OPro.Priority='B')
	INNER JOIN dbo.OpportunityPersons OP ON OP.RelationTypeId = 2 
										AND OP.OpportunityId=O.OpportunityId 
	INNER JOIN dbo.Person Per ON OP.PersonId=Per.PersonId
	INNER JOIN dbo.Calendar CAL ON OP.NeedBy between CAL.MonthStartDate AND CAL.MonthEndDate
	GROUP BY CAL.MonthStartDate,
			 C.ClientId,
			 C.Name,
			 O.OpportunityId,
			 O.OpportunityNumber,
			 OPro.DisplayName,
			 O.Name,
			 O.Description,
			 Per.PersonId,
			 Per.FirstName,
			 Per.LastName,
			 OP.NeedBy

