CREATE PROCEDURE [dbo].[DeclineProjectBudgetReset]
(
    @UserAlias          NVARCHAR(255),
	@RequestId			INT = 0,
	@Comments			NVARCHAR(MAX)
)
AS

	DECLARE @CurrentPMTime DATETIME,
			@RequestIdLocal INT,
			@UpdatedBy INT, 
			@RequesterAlias NVARCHAR(255) =''
		
	 SELECT @UpdatedBy = PersonId FROM dbo.Person WHERE Alias = @UserAlias
	 SELECT @CurrentPMTime = dbo.GettingPMTime(GETUTCDATE()),
			@RequestIdLocal=@RequestId
			

  IF EXISTS(SELECT 1 FROM dbo.BudgetResetRequestHistory WHERE RequestId=@RequestId)
	BEGIN
		EXEC dbo.SessionLogPrepare @UserLogin = @UserAlias
		INSERT INTO dbo.BudgetResetDeclineHistory(RequestId, DeclinedBy, DeclinedDate, Comments)
		VALUES (@RequestIdLocal,@UpdatedBy,@CurrentPMTime, @Comments)
		EXEC dbo.SessionLogUnprepare

		SELECT @RequesterAlias=p.Alias
		FROM dbo.BudgetResetRequestHistory b
		JOIN dbo.Person p on p.PersonId=b.RequestedBy
		WHERE b.RequestId=@RequestId
	END

SELECT  @RequesterAlias

