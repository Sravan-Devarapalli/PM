CREATE PROCEDURE [dbo].[GetActiveAndAtRiskProjectsByClient]
(
	@ClientId	INT,
	@UserLogin	NVARCHAR(255)
)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @UserHasHighRoleThanProjectLead INT = NULL
	DECLARE @PersonId INT

	SELECT @PersonId = P.PersonId
	FROM Person P
	WHERE P.Alias = @UserLogin

	IF EXISTS ( SELECT 1
				FROM aspnet_Users U
				JOIN aspnet_UsersInRoles UR ON UR.UserId = U.UserId
				JOIN aspnet_Roles R ON R.RoleId = UR.RoleId
				WHERE U.UserName = @UserLogin AND R.LoweredRoleName = 'project lead')
	BEGIN
		SET @UserHasHighRoleThanProjectLead = 0
		
		SELECT @UserHasHighRoleThanProjectLead = COUNT(*)
		FROM aspnet_Users U
		JOIN aspnet_UsersInRoles UR ON UR.UserId = U.UserId
		JOIN aspnet_Roles R ON R.RoleId = UR.RoleId
		WHERE U.UserName = @UserLogin
		AND R.LoweredRoleName IN ('system administrator','client director','business unit manager','salesperson','practice area manager','senior leadership','operations')			
	END
	
    SELECT  P.ProjectId,
			P.Name AS 'ProjectName',
			P.ProjectNumber,
			PS.Name AS ProjectStatusName
    FROM dbo.Project P
	INNER JOIN dbo.ProjectStatus AS PS ON PS.ProjectStatusId = P.ProjectStatusId
    WHERE P.ClientId = @ClientId AND P.ProjectStatusId IN (3,8,4) -- active, atrisk, completed
		  AND (@UserHasHighRoleThanProjectLead IS NULL
					OR @UserHasHighRoleThanProjectLead > 0
					OR (@UserHasHighRoleThanProjectLead = 0
						AND ( EXISTS (SELECT 1 FROM dbo.ProjectAccess projManagers2 WHERE projManagers2.ProjectId = P.ProjectId AND projManagers2.ProjectAccessId = @PersonId) OR P.SalesPersonId = @PersonId)
						)
				)
	ORDER BY P.ProjectNumber
END

