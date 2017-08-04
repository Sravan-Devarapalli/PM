﻿CREATE PROCEDURE [dbo].[SendMarginExceptionRequest]
(
	@ProjectId	INT,
	@UserAlias  NVARCHAR(100),
	@TargetMargin DECIMAL(5,2),
	@TierTwoStatus INT,
	@TargetRevenue DECIMAL(18,2),
	@Comments   NVARCHAR(MAX),
	@IsRevenueException BIT,
	@Recipient	NVARCHAR(100) OUT
)
AS
BEGIN
	
	    DECLARE @ProjectIdLocal INT =@ProjectId,
			    @Approver       INT,
			    @UserId			INT,
			    @CurrentPMTime  DATETIME

		SELECT @CurrentPMTime = dbo.GettingPMTime(GETUTCDATE())
		SELECT @userId = PersonId FROM dbo.Person WHERE Alias = @UserAlias

		SELECT @Approver=PRD.DivisionOwnerId
		FROM Project P
		JOIN ProjectDivision PD ON PD.DivisionId=P.DivisionId
		JOIN PersonDivision PRD ON PD.DivisionName=PRD.DivisionName

		EXEC dbo.SessionLogPrepare @UserLogin = @UserAlias

		--status 1- Request, 2- Accept, 3- Decline

		INSERT INTO dbo.MarginExceptionRequest(ProjectId,Requestor,Approver,TargetMargin,RequestDate,TierOneStatus,TierTwoStatus, TargetRevenue, IsRevenueException)
		VALUES(@ProjectIdLocal,@UserId,@Approver,@TargetMargin,@CurrentPMTime,1, @TierTwoStatus, @TargetRevenue, @IsRevenueException)

		select @Recipient=Alias from person where PersonId=@Approver

		EXEC dbo.SessionLogUnprepare

END