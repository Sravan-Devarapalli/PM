CREATE PROCEDURE [Skills].[GetPersonProfiles]
(
	@PersonId         INT
)
AS
BEGIN

	SELECT pp.Id AS [ProfileId],
		   pp.ProfileName,
		   pp.ProfileUrl,
		   p.personId AS [ModifiedBy],
		   p.LastName +' '+ p.FirstName AS [ModifiedByName],
		   dbo.GettingPMTime(pp.ModifiedDate) AS [ModifiedDate],
		   pp.IsDefault
	FROM [Skills].[PersonProfile] pp
	INNER JOIN dbo.Person p ON p.PersonId = pp.ModifiedBy
	WHERE pp.PersonId = @PersonId
	ORDER BY pp.ProfileName
END