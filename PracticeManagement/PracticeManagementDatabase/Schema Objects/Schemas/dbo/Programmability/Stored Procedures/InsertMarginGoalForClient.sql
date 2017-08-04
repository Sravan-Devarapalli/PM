CREATE PROCEDURE [dbo].[InsertMarginGoalForClient]
(
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

		INSERT INTO ClientMarginGoal(ClientId, StartDate, EndDate, MarginGoal, Comments)
		VALUES(@ClientId,@StartDate,@EndDate,@MarginGoal,@Comments)

		EXEC dbo.SessionLogUnprepare
END

