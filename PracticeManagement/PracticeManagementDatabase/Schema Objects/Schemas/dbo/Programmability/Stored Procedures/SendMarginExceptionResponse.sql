CREATE PROCEDURE [dbo].[SendMarginExceptionResponse]
(
	@RequestId	INT,
	@Status		INT,
	@UserAlias  NVARCHAR(100),
	@TierTwoStatus INT,
	@Comments   NVARCHAR(MAX)
)
AS
BEGIN

	    DECLARE @UserId			INT,
			    @CurrentPMTime  DATETIME,
				@Recipient	NVARCHAR(100)

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


		EXEC dbo.SessionLogUnprepare

		declare @CFOAlias NVARCHAR(MAX)=''

		-- TierTwo Approver
		select @CFOAlias=@CFOAlias+','+ p.Alias
		FROM Person P
		JOIN Title T on P.TitleId =T.TitleId
		WHERE T.Title = 'Chief Financial Officer' AND P.PersonStatusId = 1 --Active

		SELECT M.RequestReason as Reason ,
			   M.Comments,
			   P.FirstName+', '+p.LastName as Requester,
			   @CFOAlias as CFOAlias,
			   M.TierTwoStatus
		FROM MarginExceptionRequest M
		JOIN Person p on m.Requestor = p.PersonId
		WHERE M.Id=@RequestId AND M.TierOneStatus = 2 AND M.TierTwoStatus = 1

		SELECT Alias as RequestorAlias
		FROM Person P
		JOIN MarginExceptionRequest M on M.Requestor=P.PersonId
		WHERE M.Id=@RequestId
END
