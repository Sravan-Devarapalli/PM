CREATE PROCEDURE [dbo].[GetPersonEmploymentHistoryById]
(
	@PersonId INT
)
AS
BEGIN
	
	SELECT PH.PersonId,
			PH.HireDate,
			PH.TerminationDate,
			PH.TerminationReasonId
	FROM v_PersonHistory PH
	WHERE PH.PersonId = @PersonId AND PH.HireDate IS NOT NULL

END
