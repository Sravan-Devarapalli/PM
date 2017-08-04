CREATE PROCEDURE [dbo].[GetMarginGoalsForClient]
(
	@ClientId	INT
)
AS
BEGIN
	SELECT CM.Id,
		   CM.ClientId,
		   CM.StartDate,
		   CM.EndDate,
		   CM.MarginGoal,
		   CM.Comments
	FROM ClientMarginGoal CM
	WHERE CM.ClientId=@ClientId
END

