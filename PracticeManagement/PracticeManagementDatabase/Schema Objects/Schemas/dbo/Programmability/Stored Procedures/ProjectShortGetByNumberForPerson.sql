CREATE PROCEDURE [dbo].[ProjectShortGetByNumberForPerson]
(
	@ProjectNumber NVARCHAR(12),
	@UserLogin	   NVARCHAR(255)
)
AS
BEGIN

	SET NOCOUNT ON;

	
	DECLARE @PersonId INT,
		    @ProjectNumberLocal NVARCHAR(12), 
			@ProjectId INT = NULL

	select @ProjectNumberLocal = RTRIM(@ProjectNumber)

	SELECT @PersonId = P.PersonId
	FROM Person P
	WHERE P.Alias = @UserLogin

	DECLARE @PersonRole BIT=0
	
	IF EXISTS(SELECT 1 FROM v_UsersInRoles AS uir
    WHERE uir.PersonId = @PersonId and (uir.RoleName = 'System Administrator' OR uir.RoleName = 'Operations'))
	BEGIN
		SET @PersonRole = 1
	END
	
	IF @PersonRole = 1 
		SET @PersonId = null
	
	SELECT  @ProjectId = P.ProjectId
	    FROM  dbo.Project AS P
		WHERE  P.ProjectNumber = @ProjectNumberLocal
				AND @ProjectNumberLocal != 'P999918' AND P.IsInternal=0
	
	IF ( @ProjectId IS NOT NULL ) 
	BEGIN
	
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

			-- Convert client ids from string to table
			DECLARE @ClientsList TABLE (Id INT)
			INSERT INTO @ClientsList
			SELECT TargetId FROM dbo.PersonClientPermissions(@PersonId)

			-- Convert practice ids from string to table
			DECLARE @PracticesList TABLE (Id INT)
			INSERT INTO @PracticesList
			SELECT TargetId FROM dbo.PersonPracticePermissions(@PersonId)

			-- Convert project group ids from string to table
			DECLARE @ProjectGroupsList TABLE (Id INT)
			INSERT INTO @ProjectGroupsList
			SELECT TargetId FROM dbo.PersonProjectGroupPermissions(@PersonId)

			-- Convert project group ids from string to table
			DECLARE @ProjectOwnerList TABLE (Id INT)
			INSERT INTO @ProjectOwnerList
			SELECT TargetId FROM dbo.PersonProjectOwnerPermissions(@PersonId)

			DECLARE @OwnerProjectClientList TABLE(
				ClientId INT NULL -- As per #2890
			)

		   -- Table variable to store list of groups that owner and salesPerson is allowed to see	
			DECLARE @OwnerProjectGroupList TABLE(
				GroupId INT NULL -- As per #2890
			)

			-- Populate is with the data from the Project
			INSERT INTO @OwnerProjectClientList (ClientId) 
			SELECT proj.ClientId 
			FROM dbo.Project AS proj
			INNER JOIN ProjectAccess AS projManagers ON projManagers.ProjectId = proj.ProjectId
			WHERE projManagers.ProjectAccessId = @PersonId 
					OR proj.SalesPersonId = @PersonId -- Adding Salesperson - Project clients into the list.
					OR proj.ProjectManagerId = @PersonId

			INSERT INTO @OwnerProjectGroupList (GroupId) 
			SELECT GroupId FROM  dbo.ProjectGroup
			WHERE ClientId IN (SELECT opc.ClientId FROM @OwnerProjectClientList AS opc)


			 SELECT P.ProjectId,
					P.ProjectNumber,
				   P.StartDate,
				   P.EndDate,
				   P.Name,
				   PS.ProjectStatusId,
				   PS.Name AS ProjectStatusName
			INTO #SelectedProject
			FROM dbo.Project AS P
			INNER JOIN dbo.ProjectStatus AS PS ON PS.ProjectStatusId = P.ProjectStatusId
			INNER JOIN ProjectAccess AS projManagers ON projManagers.ProjectId = p.ProjectId AND p.IsAllowedToShow = 1
			WHERE P.ProjectNumber = @ProjectNumberLocal 
				AND (@PersonId is NULL OR p.ClientId in (SELECT * FROM @ClientsList) OR  p.ClientId IN (SELECT opc.ClientId FROM @OwnerProjectClientList AS opc))
				AND (@PersonId is NULL OR p.GroupId in (SELECT * FROM @ProjectGroupsList) OR  P.GroupId IN (SELECT opG.GroupId FROM @OwnerProjectGroupList AS opG))
				
				AND (@PersonId is NULL OR projManagers.ProjectAccessId = @PersonId  OR p.SalesPersonId = @PersonId OR projManagers.ProjectAccessId in (SELECT * FROM @ProjectOwnerList)
										OR p.ProjectManagerId = @PersonId OR p.ProjectManagerId in (select * from @ProjectOwnerList))
				AND ( @UserHasHighRoleThanProjectLead IS NULL 
						OR @UserHasHighRoleThanProjectLead <> 0
						OR (@UserHasHighRoleThanProjectLead = 0 AND (projManagers.ProjectAccessId = @PersonId OR p.SalesPersonId = @PersonId OR P.ProjectManagerId = @PersonId ))
					)

			IF EXISTS ( SELECT 1 FROM #SelectedProject)
			BEGIN
				SELECT * FROM #SelectedProject
			END
			ELSE
			BEGIN
				RAISERROR('User does not have access to this project.', 16, 1)
			END 
	END
	ELSE 
	BEGIN
		RAISERROR('There is no Project with this Project Number.', 16, 1)
	END
END
