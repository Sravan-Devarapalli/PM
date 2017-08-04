CREATE PROCEDURE [dbo].[GetClientMarginGoalLog]
(
	@ClientId INT
)
AS
BEGIN

		SELECT C.ClientId,
			   C.Activity,
			   C.LogTime as LogDate,
			   C.OldStartDate,
			   C.NewStartDate,
			   C.OldEndDate,
			   C.NewEndDate,
			   C.OldMarginGoal,
			   C.NewMarginGoal,
			   C.Comments,
			   p.LastName+', '+p.FirstName as PersonName
		FROM ClientMarginGoalHistory C
		JOIN Person p ON C.PersonId =p.PersonId
		WHERE C.ClientId = @ClientId

END
