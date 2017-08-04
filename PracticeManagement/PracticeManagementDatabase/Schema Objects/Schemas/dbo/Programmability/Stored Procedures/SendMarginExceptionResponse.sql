CREATE PROCEDURE [dbo].[SendMarginExceptionResponse]
(
	@RequestId	INT,
	@Status		INT,
	@UserAlias  NVARCHAR(100),
	@TierTwoStatus INT,
	@Comments   NVARCHAR(MAX),
	@Recipient	NVARCHAR(100) OUT
)
AS
BEGIN

	    DECLARE @UserId			INT,
			    @CurrentPMTime  DATETIME

		SELECT @CurrentPMTime = dbo.GettingPMTime(GETUTCDATE())
		SELECT @userId = PersonId FROM dbo.Person WHERE Alias = @UserAlias
		EXEC dbo.SessionLogUnprepare

		EXEC dbo.SessionLogPrepare @UserLogin = @UserAlias

		--status 1- Request, 2- Accept, 3- Decline

		UPDATE M
			SET M.ApprovedBy=@UserId,
				M.TierOneStatus= CASE WHEN @TierTwoStatus=1 THEN M.TierOneStatus ELSE @Status END,
				M.TierTwoStatus= CASE WHEN @TierTwoStatus=1 THEN @Status ELSE M.TierTwoStatus END,
				M.ResponseDate=@CurrentPMTime
		FROM MarginExceptionRequest M
		WHERE M.Id=@RequestId
		EXEC dbo.SessionLogUnprepare
		
		EXEC dbo.SessionLogPrepare @UserLogin = @UserAlias

		UPDATE P
			SET p.ExceptionMargin=case when m.IsRevenueException=0 then M.TargetMargin ELSE NULL END,
			p.ExceptionRevenue=M.TargetRevenue
		FROM Project p
		JOIN MarginExceptionRequest M on M.Projectid=P.ProjectId
		WHERE M.Id=@RequestId and M.TierOneStatus=2 and (M.TierTwoStatus=2 OR M.TierTwoStatus=0)
		EXEC dbo.SessionLogUnprepare

		SELECT @Recipient = Alias 
		FROM Person P
		JOIN MarginExceptionRequest M on M.Requestor=P.PersonId
		WHERE M.Id=@RequestId

		EXEC dbo.SessionLogUnprepare
END
