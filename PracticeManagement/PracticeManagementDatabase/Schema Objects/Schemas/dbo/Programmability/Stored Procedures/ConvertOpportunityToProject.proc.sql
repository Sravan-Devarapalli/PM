﻿CREATE PROCEDURE [dbo].[ConvertOpportunityToProject]
(
	@OpportunityId   INT,
	@UserLogin       NVARCHAR(255),
	@HasPersons		 BIT,
	@ProjectID		 INT OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @ClientId INT
	DECLARE @Discount DECIMAL(18,2)
	DECLARE @Terms INT
	DECLARE @PracticeId INT
	DECLARE @Name NVARCHAR(100)
	DECLARE @ProjectedStartDate DATETIME
	DECLARE @ProjectedEndDate DATETIME
	DECLARE @MilestoneId INT
	DECLARE @GroupId INT
	DECLARE @PricingListId INT
	DECLARE @SalespersonId INT
	DECLARE @OpportunityStatusId INT
	DECLARE @BuyerName NVARCHAR(100)
	DECLARE @IsChargeable BIT
	DECLARE @ProjectManagerId NVARCHAR(255)
	DECLARE @OpportunityPerson NVARCHAR(100)
	DECLARE @MilestonePersonId INT
	DECLARE @Description NVARCHAR(MAX)
	DECLARE @BusinessTypeId INT

	SELECT TOP 1
		   @ClientId = o.ClientId,
	       @Discount = o.Discount,
	       @Terms = o.Terms,
	       @PracticeId = o.PracticeId,
	       @Name = o.Name,
	       @ProjectedStartDate = o.ProjectedStartDate,
	       @ProjectedEndDate = o.ProjectedEndDate,
	       @SalespersonId = o.SalespersonId,
	       @OpportunityStatusId = o.OpportunityStatusId,
	       @BuyerName = o.BuyerName,
	       @GroupId = ISNULL(o.GroupId, (SELECT PG.GroupId FROM dbo.ProjectGroup PG WHERE PG.ClientId = o.ClientId and PG.Code = 'B0001')),
		   @PricingListId=o.PricingListId,
		   @BusinessTypeId=O.BusinessTypeId,
	       @IsChargeable = 1,
	       @ProjectManagerId = CONVERT(NVARCHAR(255),ISNULL(o.OwnerId, pr.PracticeManagerId)),
		   @Description =Description
	  FROM dbo.v_Opportunity AS o
	  inner join dbo.Practice as pr on pr.PracticeId = o.PracticeId
	 WHERE o.OpportunityId = @OpportunityId

	IF @OpportunityStatusId = 4 /* Won */
	BEGIN
		RAISERROR('Cannot convert an opportunity with the status Won to project.', 16, 1)
		RETURN
	END

	-- Create a project
	EXEC dbo.ProjectInsert @ClientId = @ClientId,
		@Terms = @Terms,
		@Name = @Name,
		@PracticeId = @PracticeId,
		@ProjectStatusId = 2 /* Projected */,
		@BuyerName = @BuyerName,
		@UserLogin = @UserLogin,
		@GroupId = @GroupId,
		@PricingListId=@PricingListId,
		@ProjectManagerIdsList = @ProjectManagerId,
	    @ProjectId = @ProjectId OUTPUT,
		@OpportunityId = @OpportunityId,
		@Description   = @Description,
		@CanCreateCustomWorkTypes = 0,
		@IsInternal = 0,
		@SowBudget = NULL,
		@BusinessTypeId=@BusinessTypeId,
		@SalesPersonId = @SalesPersonId

	IF(@HasPersons = 1)
	BEGIN
	
	-- Create a milestone
	EXEC dbo.MilestoneInsert @ProjectId = @ProjectId,
		@Description = 'Milestone 1',
		@Amount = NULL,
		@StartDate = @ProjectedStartDate,
		@ProjectedDeliveryDate = @ProjectedEndDate,
		@IsHourlyAmount = 1,
		@UserLogin = @UserLogin,
		@ConsultantsCanAdjust = 0,
		@IsChargeable = @IsChargeable,
		@MilestoneId = @MilestoneId OUTPUT
	
	-- Add persons to milestone
		DECLARE @OpportunityPersons TABLE(OpportunityId INT, PersonId INT, RowNumber INT ,NeedBy DateTime,Quantity INT)
		DECLARE @PersonsCount INT = (SELECT COUNT(PersonId) FROM dbo.OpportunityPersons WHERE OpportunityId = @OpportunityId AND OpportunityPersonTypeId = 1)
		DECLARE @tempPersonId INT
		DECLARE @tempPersonQuantity INT
		DECLARE @tempPersonStartDate date 
		DECLARE @Index INT = 1
		DECLARE @Index1 INT = 1
		
		INSERT INTO @OpportunityPersons(RowNumber, OpportunityId, PersonId, NeedBy ,Quantity)
		SELECT	ROW_NUMBER() OVER(ORDER BY personId,needby) AS RowNumber
				, OpportunityId
				, PersonId
				, NeedBy
				,Quantity
		FROM dbo.OpportunityPersons
		WHERE OpportunityId = @OpportunityId AND OpportunityPersonTypeId = 1
				
		WHILE @Index <= @PersonsCount
		BEGIN
			SELECT @tempPersonId = PersonId ,
					@tempPersonStartDate = ISNULL(NeedBy, @ProjectedStartDate),
					@tempPersonQuantity = ISNULL(Quantity,1)
			FROM @OpportunityPersons WHERE RowNumber = @Index

			EXEC dbo.MilestonePersonInsert  @MilestoneId = @MilestoneId,
				@PersonId = @tempPersonId,
				@MilestonePersonId = @MilestonePersonId OUTPUT
			SET @Index1 = 1;
			WHILE @Index1 <= @tempPersonQuantity
			BEGIN
			
			EXEC dbo.MilestonePersonEntryInsert @PersonId = @tempPersonId,
				@MilestonePersonId = @MilestonePersonId, 
				@StartDate= @tempPersonStartDate, 
				@EndDate = @ProjectedEndDate, 
				@HoursPerDay = 8,
				@PersonRoleId = NULL,
				@Amount = NULL,
				@Location = NULL,
				@UserLogin = @UserLogin
				
			SET @Index1 = @Index1 + 1;
			END
			SET @Index = @Index + 1;
		END 
	END

	-- Set opportunity status
	-- As per #3083 opportunity status is not changed
	UPDATE dbo.Opportunity
	   SET ProjectId = @ProjectID
	 WHERE OpportunityId = @OpportunityId

	DECLARE @PersonId INT
	SELECT @PersonId = p.PersonId FROM dbo.Person AS p WHERE p.Alias = @UserLogin

	DECLARE @CreatedMessage NVARCHAR(100)
	SELECT @CreatedMessage = 'Opportunity Won - Project created: ' + ProjectNumber
	  FROM dbo.Project AS p
	 WHERE p.ProjectId = @ProjectId

	-- Add an opportunity history
	EXEC dbo.OpportunityTransitionInsert @OpportunityId = @OpportunityId,
		@OpportunityTransitionStatusId = 2,
		@PersonId = @PersonId,
		@NoteText = @CreatedMessage,
		@OpportunityTransitionId = null
END

