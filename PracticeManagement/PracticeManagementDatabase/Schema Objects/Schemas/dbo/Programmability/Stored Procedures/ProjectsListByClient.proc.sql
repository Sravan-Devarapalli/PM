﻿CREATE PROCEDURE  dbo.ProjectsListByClient
(
	@ClientId INT,
	@Alias VARCHAR(100) = NULL 
)
AS
	SET NOCOUNT ON

	IF @Alias IS NULL   
		SELECT p.ClientId,
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
			   p.GroupId,
			   p.ClientIsChargeable,
			   p.ProjectIsChargeable,
			   p.ProjectManagersIdFirstNameLastName,
			   p.ExecutiveInChargeId AS DirectorId,
			   p.DirectorFirstName,
			   p.DirectorLastName
		  FROM dbo.v_Project AS p
		 WHERE p.ClientId = @ClientId AND p.IsAllowedToShow = 1
		ORDER BY p.Name
	ELSE 
	BEGIN 
		DECLARE @PersonId INT 
		
		SELECT @PersonId = PersonId 
		FROM dbo.Person
		WHERE Alias = @Alias;
	
		WITH Perms AS (
			SELECT TargetId,
				   TargetType
			FROM dbo.Permission 
			WHERE PersonId = @PersonId
		)
		SELECT p.ClientId,
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
			   p.GroupId,
			   p.ClientIsChargeable,
			   p.ProjectIsChargeable,
			   p.ProjectManagersIdFirstNameLastName,
			   p.ExecutiveInChargeId AS DirectorId,
			   p.DirectorFirstName,
			   p.DirectorLastName
		  FROM dbo.v_Project AS p
		 WHERE 
			p.ClientId = @ClientId AND p.IsAllowedToShow = 1 AND 
			(
				p.GroupId IN (SELECT TargetId FROM Perms AS s WHERE s.TargetType = 2) OR
				p.PracticeId IN (SELECT TargetId FROM Perms AS s WHERE s.TargetType = 5) OR
				p.PracticeManagerId IN (SELECT TargetId FROM Perms AS s WHERE s.TargetType = 4) OR
				p.SalesPersonId IN (SELECT TargetId FROM Perms AS s WHERE s.TargetType = 3)
			)
		ORDER BY p.NAME
	END 

