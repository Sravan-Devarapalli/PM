﻿CREATE PROCEDURE dbo.PersonListSalesperson
    (
      @IncludeInactive BIT ,
      @PersonId INT = NULL
    )
AS 
BEGIN
    SET NOCOUNT ON;
	/*
		Showing Sales Person of the Projects, Where the Logged in User is Project Manager OR Sales Person of 
		the Project(applicable only for Project Lead role Person) as part of #2941.
	*/

	DECLARE @UserHasHighRoleThanProjectLead INT = NULL
	--Adding Project Lead as per #2941.
	IF @PersonId IS NOT NULL AND EXISTS ( SELECT 1
				FROM aspnet_Users U
				JOIN aspnet_UsersInRoles UR ON UR.UserId = U.UserId
				JOIN aspnet_Roles R ON R.RoleId = UR.RoleId
				JOIN Person P ON P.Alias = U.UserName
				WHERE P.PersonId = @PersonId AND R.LoweredRoleName = 'project lead')
	BEGIN
		SET @UserHasHighRoleThanProjectLead = 0
		
		SELECT @UserHasHighRoleThanProjectLead = COUNT(*)
		FROM aspnet_Users U
		JOIN aspnet_UsersInRoles UR ON UR.UserId = U.UserId
		JOIN aspnet_Roles R ON R.RoleId = UR.RoleId
		JOIN Person P ON P.Alias = U.UserName
		WHERE P.PersonId = @PersonId
			AND R.LoweredRoleName IN ('system administrator','client director','business unit manager','salesperson','practice area manager','senior leadership','operations')			
	END

	IF @UserHasHighRoleThanProjectLead = 0--Adding Project Lead as per #2941.
	BEGIN

		SELECT DISTINCT SP.PersonId ,
				SP.FirstName ,
				SP.PreferredFirstName,
				SP.LastName ,
				SP.HireDate ,
				SP.TerminationDate ,
				SP.Alias ,
				SP.DefaultPractice ,
				SP.PracticeName ,
				SP.PersonStatusId ,
				SP.PersonStatusName ,
				SP.EmployeeNumber ,
				SP.SeniorityId ,
				SP.SeniorityName ,
				SP.ManagerId ,
				SP.ManagerAlias ,
				SP.ManagerFirstName ,
				SP.ManagerLastName ,
				SP.PracticeOwnedId ,
				SP.PracticeOwnedName,
				SP.TelephoneNumber
		FROM Project P
		LEFT JOIN ProjectAccess PM ON PM.ProjectId = P.ProjectId
		JOIN v_Person SP ON SP.PersonId = P.SalesPersonId
		WHERE (PM.ProjectAccessId = @PersonId OR P.SalesPersonId = @PersonId OR P.ProjectManagerId = @PersonId) --Logged in User should be Project Manager or Sales Person of the Project.
			AND (SP.PersonStatusId IN (1,5) /* Active person only */
					OR @IncludeInactive = 1)
			AND sp.IsStrawman = 0

	END
	ELSE
	BEGIN

	   DECLARE @Now DATETIME
		SET @Now = dbo.Today()

		-- Table variable to store list of salespersons user is allowed to see	
		DECLARE @SalespersonPermissions TABLE ( PersonId INT NULL )
		-- Populate is with the data from the permissions table
		--		TargetType = 3 means that we are looking for the salespersons in the permissions table
		INSERT  INTO @SalespersonPermissions
				( PersonId
			  )
				SELECT  prm.TargetId
				FROM    v_permissions AS prm
				WHERE   prm.PersonId = @PersonId
						AND prm.PermissionTypeId = 3
                    
		-- Table variable to store list of SalesPersons that owner is allowed to see	
		DECLARE @OwnerProjectSalesPersonList TABLE(
			PersonId INT NULL -- As per #2890
		)
	
		-- If there are nulls in permission table, it means that eveything is allowed to be seen,
		--		so set @PersonId to NULL which will extract all records from the table
		DECLARE @NullsNumber INT
		SELECT  @NullsNumber = COUNT(*)
		FROM    @SalespersonPermissions AS sp
		WHERE   sp.PersonId IS NULL 

		IF @NullsNumber > 0 
		BEGIN
			SET @PersonId = NULL
		END
		ELSE
		BEGIN
			INSERT INTO @OwnerProjectSalesPersonList (PersonId) 
			SELECT proj.SalesPersonId
			FROM dbo.Project AS proj 
			INNER JOIN  dbo.ProjectAccess AS projManagers ON proj.ProjectId = projManagers.ProjectId
			WHERE projManagers.ProjectAccessId = @PersonId 
				OR proj.SalesPersonId = @PersonId 
				OR proj.ProjectManagerId = @PersonId 
		END

			;WITH    Salespersons
					  AS ( SELECT   p.PersonId ,
									p.FirstName ,
									P.PreferredFirstName,
									p.LastName ,
									p.HireDate ,
									p.TerminationDate ,
									p.Alias ,
									p.DefaultPractice ,
									p.PracticeName ,
									p.PersonStatusId ,
									p.PersonStatusName ,
									p.EmployeeNumber ,
									p.SeniorityId ,
									p.SeniorityName ,
									p.ManagerId ,
									p.ManagerAlias ,
									p.ManagerFirstName ,
									p.ManagerLastName ,
									p.PracticeOwnedId ,
									p.PracticeOwnedName,
									p.TelephoneNumber
						   FROM     dbo.v_Person AS p
									INNER JOIN dbo.aspnet_Users AS u ON p.Alias = u.UserName
									INNER JOIN dbo.aspnet_UsersInRoles AS ur ON u.UserId = ur.UserId
									INNER JOIN dbo.aspnet_Roles AS r ON ur.RoleId = r.RoleId
						   WHERE    r.RoleName = 'Salesperson' AND 
									(p.PersonStatusId IN (1,3,5))
									AND ( @PersonId IS NULL
										  OR p.PersonId IN (
										  SELECT sp.PersonId
										  FROM   @SalespersonPermissions AS sp)
										  OR p.PersonId IN (
										  SELECT    opsp.PersonId
										  FROM      @OwnerProjectSalesPersonList AS opsp )
										)
						 )
			SELECT  DISTINCT *
			FROM    Salespersons AS s
			ORDER BY s.LastName ,
					s.FirstName
	END
END

