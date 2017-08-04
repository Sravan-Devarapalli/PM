CREATE PROCEDURE [dbo].[UpdateMarginGoal]
(
	@Id			INT,
	@ClientId	INT,
	@StartDate	DATETIME,
	@EndDate		DATETIME,
	@MarginGoal	INT,
	@Comments    NVARCHAR(MAX) = NULL,
	@UserLogin                NVARCHAR(255)
)
AS
BEGIN
		SET NOCOUNT ON;

		EXEC dbo.SessionLogPrepare @UserLogin = @UserLogin

		UPDATE CM
		   SET CM.StartDate=@StartDate,
			   CM.EndDate=@EndDate,
			   CM.MarginGoal=@MarginGoal,
			   CM.Comments=@Comments
		FROM ClientMarginGoal CM
		WHERE CM.Id=@Id AND CM.ClientId=@ClientId

		EXEC dbo.SessionLogUnprepare
END
