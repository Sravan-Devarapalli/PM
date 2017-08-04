CREATE PROCEDURE dbo.MilestonePersonEntryInsert
(
	@PersonId            INT = NULL,
	@MilestonePersonId   INT,
	@StartDate	   DATETIME,
	@EndDate       DATETIME,
	@HoursPerDay   DECIMAL(4,2),
	@PersonRoleId  INT,
	@Amount        DECIMAL(18,2),
	@IsBadgeRequired	BIT = 0,
	@BadgeStartDate		DATETIME = NULL,
	@BadgeEndDate		DATETIME = NULL,
	@IsBadgeException	BIT = 0,
	@IsApproved			BIT = 0,
	@Location      NVARCHAR(20) = NULL,
	@UserLogin     NVARCHAR(255),
	@Id            INT = NULL OUTPUT,
	@Discount	   DECIMAL(18,2)=NULL,
	@IsNewToBudget  BIT = 0,
	@IsDiscountAtResourceLevel INT =0,
	@LockDiscount   BIT = 0
)
AS
	SET NOCOUNT ON

	BEGIN TRY
	BEGIN TRAN  Tran_MilestonePersonEntryInsrt

	-- Start logging session
	EXEC dbo.SessionLogPrepare @UserLogin = @UserLogin

	DECLARE @UpdatedBy  INT,
			@RequestDate DATETIME=NULL,
			@CurrentPMTime DATETIME,
			@ProjectId INT,
			@IsNewToBudgetLocal BIT,
			@MilestoneId INT

	SELECT @ProjectId = M.ProjectId
	FROM dbo.MilestonePerson MP
	JOIN dbo.Milestone M ON M.MilestoneId = MP.MilestoneId
	WHERE MP.MilestonePersonId = @MilestonePersonId

	SELECT @UpdatedBy = PersonId FROM dbo.Person WHERE Alias = @UserLogin

	SELECT @CurrentPMTime = dbo.GettingPMTime(GETUTCDATE())
	IF(@IsBadgeRequired = 1)
	BEGIN
		SET @RequestDate = @CurrentPMTime
	END

	--to track if the resource is added after the budget is set
	IF EXISTS(SELECT 1 FROM dbo.Project WHERE ProjectId=@ProjectId AND ProjectStatusId = 3 AND Budget IS NOT NULL)
		SET @IsNewToBudgetLocal=CONVERT(bit, 1)
	ELSE 
		SET @IsNewToBudgetLocal=CONVERT(bit, 0)


	INSERT INTO dbo.MilestonePersonEntry
	            (MilestonePersonId, StartDate, EndDate, PersonRoleId, Amount,IsBadgeRequired,BadgeStartDate,BadgeEndDate,IsBadgeException,IsApproved,BadgeRequestDate, HoursPerDay, Location,Requester, Discount, IsNewToBudget, LockDiscount)
	     VALUES (@MilestonePersonId, @StartDate, @EndDate, @PersonRoleId, @Amount,@IsBadgeRequired,@BadgeStartDate,@BadgeEndDate,@IsBadgeException,@IsApproved,@RequestDate, @HoursPerDay, @Location,@UpdatedBy, @Discount, @IsNewToBudgetLocal, @LockDiscount)

		SET @Id = SCOPE_IDENTITY()
	     
	IF @PersonId IS NOT NULL
	BEGIN 
		UPDATE dbo.MilestonePerson
		SET PersonId = @PersonId
		WHERE MilestonePersonId = @MilestonePersonId
	END

	SELECT @MilestoneId = MilestoneId
	FROM MilestonePerson WHERE MilestonePersonId = @MilestonePersonId
	
	IF @MilestonePersonId IS NOT NULL
	BEGIN
		EXEC dbo.InsertProjectFeedbackByMilestonePersonId @MilestonePersonId = @MilestonePersonId,@MilestoneId = NULL
	END

	EXEC dbo.UpdateMSBadgeDetailsByPersonId @PersonId = @PersonId, @UpdatedBy = @UpdatedBy

	UPDATE dbo.Project
	SET CreatedDate = @CurrentPMTime
	WHERE ProjectId = @ProjectId

	-- Update Milestone Amount if the Amount is NULL For Fixed Milestone

	IF EXISTS(SELECT 1 FROM Milestone M Where M.MilestoneId = @MilestoneId AND M.IsHourlyAmount = 0 AND M.Amount IS NULL)
	BEGIN
		EXEC UpdateFixedMilestoneAmount @MilestoneId= @MilestoneId
	END

	IF EXISTS(SELECT 1 FROM Milestone M Where M.MilestoneId = @MilestoneId AND M.IsHourlyAmount = 0 )
	BEGIN
		EXEC dbo.UpdateFixedFeeMilestoneDiscount @MilestoneId= @MilestoneId
	END


	UPDATE Milestone
	set IsDiscountAtMilestone=@IsDiscountAtResourceLevel
	where MilestoneId=@MilestoneId and @IsDiscountAtResourceLevel!=0
	 

	-- End logging session
	EXEC dbo.SessionLogUnprepare

	COMMIT TRAN Tran_MilestonePersonEntryInsrt
	END TRY
	BEGIN CATCH
		ROLLBACK TRAN Tran_MilestonePersonEntryInsrt
		DECLARE @ErrorMessage NVARCHAR(MAX)
		SET @ErrorMessage = ERROR_MESSAGE()
		RAISERROR(@ErrorMessage, 16, 1)
	END CATCH


