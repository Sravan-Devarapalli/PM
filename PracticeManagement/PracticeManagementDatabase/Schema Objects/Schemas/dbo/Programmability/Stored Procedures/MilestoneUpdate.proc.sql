CREATE PROCEDURE dbo.MilestoneUpdate
(
	@MilestoneId              INT,
	@ProjectId                INT,
	@Description              NVARCHAR(255),
	@Amount                   DECIMAL(18,2),
	@StartDate                DATETIME,
	@ProjectedDeliveryDate    DATETIME,
	@IsHourlyAmount           BIT,
	@UserLogin                NVARCHAR(255),
	@ConsultantsCanAdjust	  BIT,
	@IsChargeable			  BIT,
	@MilestoneType		      INT = 1,
	@Discount				  DECIMAL(18,2)=NULL,
	@DiscountType			  INT = NULL,
	@IsDiscountAtMilestone	  INT = 0,
	@IsAmountAtMilestone      BIT = 0
)
AS
BEGIN
	SET NOCOUNT ON;

	-- Start logging session
	EXEC dbo.SessionLogPrepare @UserLogin = @UserLogin

	DECLARE @PreviousDiscountType INT 

	SELECT @PreviousDiscountType = DiscountType
	FROM Milestone WHERE MilestoneId = @MilestoneId

	-- Change the milestone
	UPDATE dbo.Milestone
	   SET ProjectId = @ProjectId,
	       Description = @Description,
	       Amount = @Amount,
	       StartDate = @StartDate,
	       ProjectedDeliveryDate = @ProjectedDeliveryDate,
	       IsHourlyAmount = @IsHourlyAmount,
	       IsChargeable = @IsChargeable,
	       ConsultantsCanAdjust = @ConsultantsCanAdjust,
		   MilestoneType = @MilestoneType,
		   Discount= @Discount,
		   DiscountType=@DiscountType,
		   IsDiscountAtMilestone =@IsDiscountAtMilestone,
		   IsAmountAtMilestone =@IsAmountAtMilestone
	 WHERE MilestoneId = @MilestoneId

	 --Adjust Expenses in Milestone level, but not in Project level

	 UPDATE dbo.ProjectExpense 
	 SET StartDate = CASE WHEN StartDate <= @StartDate THEN @StartDate ELSE StartDate END,
	     EndDate = CASE WHEN EndDate <= @ProjectedDeliveryDate THEN EndDate ELSE @ProjectedDeliveryDate END
	 WHERE StartDate <= @ProjectedDeliveryDate AND @StartDate <= EndDate
		   AND ProjectId = @ProjectId AND MilestoneId=@MilestoneId
		   
	 UPDATE dbo.ProjectExpense 
	 SET StartDate = @StartDate,
		 EndDate = @ProjectedDeliveryDate
	 WHERE (StartDate > @ProjectedDeliveryDate OR @StartDate > EndDate)
			AND ProjectId = @ProjectId AND MilestoneId=@MilestoneId

	IF(@IsHourlyAmount = 0)
	BEGIN
		EXEC UpdateFixedMilestoneAmount @MilestoneId= @MilestoneId

		IF(@PreviousDiscountType = 1 AND @DiscountType = 2)
		BEGIN
			-- Convert dollar discount amount to percentage
			UPDATE MPE
			SET MPE.Discount =ISNULL(MPE.Discount*100/MPE.Amount,0)
			FROM MilestonePersonEntry MPE
			JOIN MilestonePerson MP ON MP.MilestonePersonId = MPE.MilestonePersonId 
			WHERE MP.MilestoneId = @MilestoneId AND MPE.Discount IS NOT NULL

		END
		ELSE IF(@PreviousDiscountType = 2 AND @DiscountType = 1)
		BEGIN
			-- Convert Percentage discount to dollar amount
			UPDATE MPE
			SET MPE.Discount =ISNULL(MPE.Discount*MPE.Amount/100,0)
			FROM MilestonePersonEntry MPE
			JOIN MilestonePerson MP ON MP.MilestonePersonId = MPE.MilestonePersonId 
			WHERE MP.MilestoneId = @MilestoneId AND MPE.Discount IS NOT NULL
		END

	END

	-- End logging session
	EXEC dbo.SessionLogUnprepare
 END

