CREATE PROCEDURE [dbo].[GetPersonListWithRole]
(
	@RoleName NVARCHAR(100)
)
AS
BEGIN

	 SELECT p.PersonId ,
            ISNULL(p.PreferredFirstName,p.FirstName) AS FirstName,
            p.LastName ,
            p.IsDefaultManager
    FROM    dbo.Person AS p
			INNER JOIN v_UsersInRoles AS ur ON ur.UserName = p.Alias AND LOWER(ur.RoleName) = LOWER(@RoleName)
    WHERE   p.PersonStatusId IN (1,5) -- Active And termination pending person only
   

	UNION 

	SELECT 0 as PersonId ,
            'Third Party' AS FirstName,
            '' as LastName ,
            CONVERT(BIT,0) as IsDefaultManager
	FROM dbo.Person AS p
	WHERE LOWER(@RoleName)=LOWER('Recruiter')
	ORDER BY LastName ,FirstName
END

