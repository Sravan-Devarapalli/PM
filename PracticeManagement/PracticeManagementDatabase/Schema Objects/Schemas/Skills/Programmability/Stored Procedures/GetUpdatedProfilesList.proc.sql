-- =============================================
-- Author:		Srinivas Middela
-- Create date: 10-05-2012
-- Last Updated by:	
-- Last Update date: 
-- Description: Reads the list of persons who updated their skills.
-- =============================================
CREATE PROCEDURE [Skills].[GetUpdatedProfilesList]
(
	@Today DATETIME
)
AS
BEGIN
	
	DECLARE @TodayLocal DATE

	SELECT @TodayLocal = @Today - 1
	
	;WITH SkillsUpdates AS
	(
		SELECT PS.PersonId
		FROM Skills.PersonSkill PS
		WHERE CONVERT(DATE, dbo.GettingPMTime(PS.ModifiedDate)) = @TodayLocal
	)
	, IndustryUpdates AS
	(
		SELECT PIn.PersonId
		FROM Skills.PersonIndustry AS PIn
		WHERE CONVERT(DATE, dbo.GettingPMTime(PIn.ModifiedDate)) = @TodayLocal
	)
	, ProfileLinkUpdates AS
	(
		SELECT PP.PersonId
		FROM Skills.PersonProfile PP
		WHERE CONVERT(DATE, dbo.GettingPMTime(PP.ModifiedDate)) = @TodayLocal
	)
	, ProfilePictureUpdates AS
	(
		SELECT P.PersonId
		FROM dbo.Person P
		WHERE CONVERT(DATE, dbo.GettingPMTime(P.PictureModifiedDate)) = @TodayLocal
	)
	, UpdatedPersons AS
	(
		SELECT *
		FROM SkillsUpdates
		UNION
		SELECT *
		FROM IndustryUpdates
		UNION
		SELECT *
		FROM ProfileLinkUpdates
		UNION
		SELECT *
		FROM ProfilePictureUpdates
	)

	SELECT P.PersonId,
			P.LastName + ', ' + P.FirstName AS PersonName
	FROM UpdatedPersons UP
	INNER JOIN dbo.Person P ON P.PersonId = UP.PersonId

END