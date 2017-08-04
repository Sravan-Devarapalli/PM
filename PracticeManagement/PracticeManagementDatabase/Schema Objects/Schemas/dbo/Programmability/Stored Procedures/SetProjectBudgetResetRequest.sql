CREATE PROCEDURE [dbo].[SetProjectBudgetResetRequest]
(
	@ProjectId INT,
	@UserAlias NVARCHAR(255),
	@Comments  NVARCHAR(MAX),
	@ResetType INT,
	@BudgetToDate DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @userId INT,
			@CurrentPMTime DATETIME
	SELECT @CurrentPMTime = dbo.GettingPMTime(GETUTCDATE())
	SELECT @userId = PersonId FROM dbo.Person WHERE Alias = @UserAlias

	EXEC dbo.SessionLogPrepare @UserLogin = @UserAlias

	 INSERT INTO dbo.BudgetResetRequestHistory(ProjectId,RequestedBy,RequestDate, Comments, ResetType, BudgetToDate)
	 VALUES (@ProjectId,@userId,@CurrentPMTime, @Comments, @ResetType, @BudgetToDate)

	 EXEC dbo.SessionLogUnprepare

END

