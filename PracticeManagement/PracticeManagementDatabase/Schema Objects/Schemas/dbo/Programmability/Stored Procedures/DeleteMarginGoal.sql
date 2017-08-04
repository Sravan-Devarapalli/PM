CREATE PROCEDURE [dbo].[DeleteMarginGoal]
(
	@Id  INT,
	@UserLogin	NVARCHAR(255)
)
AS
BEGIN

	SET NOCOUNT ON;
		
		EXEC dbo.SessionLogPrepare @UserLogin = @UserLogin

		DELETE FROM ClientMarginGoal WHERE Id=@Id

		EXEC dbo.SessionLogUnprepare
END
