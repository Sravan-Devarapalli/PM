﻿CREATE PROCEDURE [dbo].[OpportunityUpdate]
(
	@Name                  NVARCHAR(50),
	@ClientId              INT,
	@SalespersonId         INT,
	@OpportunityStatusId   INT,
	@PriorityId            INT,
	@ProjectedStartDate    DATETIME,
	@ProjectedEndDate      DATETIME,
	@CloseDate			   DATETIME,
	@Description           NVARCHAR(MAX),
	@PracticeId            INT,
	@BuyerName             NVARCHAR(100),
	@Pipeline              NVARCHAR(512),
	@Proposed              NVARCHAR(512),
	@SendOut               NVARCHAR(512),
	@OpportunityId         INT,
	@UserLogin             NVARCHAR(255),
    @ProjectId             INT,
    @OpportunityIndex      INT,
	@OwnerId			   INT = NULL,
	@GroupId			   INT,
	@EstimatedRevenue	   DECIMAL(18,2),
	@PersonIdList          NVARCHAR(MAX) = NULL,
	@StrawManList          NVARCHAR(MAX) = NULL,
	@PricingListId		   INT =NULL,
	@BusinessTypeId		   INT =NULL
)
AS
BEGIN
		
		-- Start logging session
		EXEC dbo.SessionLogPrepare @UserLogin = @UserLogin
			SET NOCOUNT ON;
			SET ANSI_NULLS ON;
		
		DECLARE @PrevOpportunityStatusId INT
		DECLARE @PrevPriority NVARCHAR(255)
		DECLARE @PrevPriorityId INT
		DECLARE @PrevPipeline NVARCHAR(1000)
		DECLARE @PrevProposed NVARCHAR(1000)
		DECLARE @PrevSendOut NVARCHAR(1000)
		DECLARE @CurrentPriority NVARCHAR(255)

		SELECT @PrevOpportunityStatusId = o.OpportunityStatusId,
			   @PrevPriority = OP.Priority,
			   @PrevPriorityId = O.PriorityId,
			   @PrevPipeline = o.Pipeline,
			   @PrevProposed = o.Proposed,
			   @PrevSendOut = o.SendOut
		  FROM dbo.Opportunity AS o
		  JOIN dbo.OpportunityPriorities AS OP ON Op.Id = o.PriorityId
		 WHERE o.OpportunityId = @OpportunityId

		 SELECT @CurrentPriority = Priority
		 FROM dbo.OpportunityPriorities
		 WHERE Id = @PriorityId

		UPDATE dbo.Opportunity
		   SET Name = @Name,
			   ClientId = @ClientId,
			   SalespersonId = @SalespersonId,
			   OpportunityStatusId = @OpportunityStatusId,
			   PriorityId = @PriorityId,
			   ProjectedStartDate = @ProjectedStartDate,
			   ProjectedEndDate = @ProjectedEndDate,
			   CloseDate = @CloseDate,
			   Description = @Description,
			   PracticeId = @PracticeId,
			   BuyerName = @BuyerName,
			   Pipeline = @Pipeline,
			   Proposed = @Proposed,
			   SendOut = @SendOut,
			   ProjectId = @ProjectId,
			   OpportunityIndex	= @OpportunityIndex,
			   OwnerId = @OwnerId,
			   GroupId = @GroupId,
			   EstimatedRevenue = @EstimatedRevenue
			   ,LastUpdated = GETDATE(),
			   PricingListId=@PricingListId,
			   BusinessTypeId=@BusinessTypeId
		 WHERE OpportunityId = @OpportunityId

		IF(@ProjectId IS NOT NULL)
		BEGIN
		  UPDATE dbo.Project
		  SET Description   = @Description,
			  OpportunityId = @OpportunityId,
			  BusinessTypeId = @BusinessTypeId,
			  PricingListId = @PricingListId
		  WHERE ProjectId = @ProjectId
		END
		ELSE
		BEGIN
	   	  UPDATE dbo.Project
			SET OpportunityId = null
			WHERE OpportunityId = @OpportunityId
		END

		-- Logging changes
		DECLARE @NoteText NVARCHAR(2000)
		DECLARE @PersonId INT
		SELECT @PersonId = PersonId FROM dbo.Person WHERE Alias = @UserLogin

		-- Determine if the status was changed
		IF @PrevOpportunityStatusId <> @OpportunityStatusId
		BEGIN
			-- Create a history record
			SELECT @NoteText = 'Status changed.  Was: ' + Name
			  FROM dbo.OpportunityStatus
			 WHERE OpportunityStatusId = @PrevOpportunityStatusId

			SELECT @NoteText = @NoteText + ' now: ' + Name
			  FROM dbo.OpportunityStatus
			 WHERE OpportunityStatusId = @OpportunityStatusId

			EXEC dbo.OpportunityTransitionInsert @OpportunityId = @OpportunityId,
				@OpportunityTransitionStatusId = 2 /* Notes */,
				@PersonId = @PersonId,
				@NoteText = @NoteText
		END

		-- Determine if the pipeline was changed
		IF ISNULL(@PrevPipeline, '') <> ISNULL(@Pipeline, '')
		BEGIN
			-- Create a history record
			SET @NoteText = 'Pipeline was: ' + ISNULL(@PrevPipeline, '') + ' now: ' + ISNULL(@Pipeline, '')

			EXEC dbo.OpportunityTransitionInsert @OpportunityId = @OpportunityId,
				@OpportunityTransitionStatusId = 6 /* Pipeline */,
				@PersonId = @PersonId,
				@NoteText = @NoteText
		END

		-- Determine if the proposed was changed
		IF ISNULL(@PrevProposed, '') <> ISNULL(@Proposed, '')
		BEGIN
			-- Create a history record
			SET @NoteText = 'Proposed was: ' + ISNULL(@PrevProposed, '') + ' now: ' + ISNULL(@Proposed, '')

			EXEC dbo.OpportunityTransitionInsert @OpportunityId = @OpportunityId,
				@OpportunityTransitionStatusId = 3 /* Proposed */,
				@PersonId = @PersonId,
				@NoteText = @NoteText
		END

		-- Determine if the send-out was changed
		IF ISNULL(@PrevSendOut, '') <> ISNULL(@SendOut, '')
		BEGIN
			-- Create a history record
			SET @NoteText = 'Send-Out was: ' + ISNULL(@PrevSendOut, '') + ' now: ' + ISNULL(@SendOut, '')

			EXEC dbo.OpportunityTransitionInsert @OpportunityId = @OpportunityId,
				@OpportunityTransitionStatusId = 5 /* Send-Out */,
				@PersonId = @PersonId,
				@NoteText = @NoteText
		END
		
		IF(@PersonIdList IS NOT NULL)
		BEGIN
			DELETE op
			FROM OpportunityPersons op
			LEFT JOIN [dbo].[ConvertStringListIntoTableWithTwoColoumns] (@PersonIdList) AS p 
			ON op.OpportunityId = @OpportunityId AND op.PersonId = p.ResultId AND op.OpportunityPersonTypeId = p.ResultType
			WHERE p.ResultId IS NULL and OP.OpportunityId = @OpportunityId AND op.RelationTypeId = 1

			INSERT INTO dbo.OpportunityPersons(OpportunityId,PersonId,OpportunityPersonTypeId,RelationTypeId)
			SELECT @OpportunityId ,p.ResultId,p.ResultType,1 -- Relation type 1 means PropesedResource
			FROM [dbo].[ConvertStringListIntoTableWithTwoColoumns] (@PersonIdList) AS p 
			LEFT JOIN OpportunityPersons op
			ON p.ResultId = op.PersonId AND op.OpportunityId=@OpportunityId
			WHERE op.PersonId IS NULL 
		END

		DELETE FROM dbo.OpportunityPersons  
		WHERE OpportunityId = @OpportunityId AND RelationTypeId = 2 -- strawmans

		IF(@StrawManList IS NOT NULL  AND ISNULL(@StrawManList,'')<>'')
		BEGIN

			DECLARE @OpportunityPersonIdsWithTypeTable TABLE
			(
			PersonId INT,
			PersonType INT,
			Quantity INT,
			NeedBy DATETIME
			)

			DECLARE @PersonIdListLocalXML XML
			-- Strawmans' info is separated  by ","s.
			-- Each Strawman info is in the format PersonId:PersonType|Quantity?NeedBy
			IF(SUBSTRING(@StrawManList,LEN(@StrawManList),1)=',')
			SET @StrawManList = SUBSTRING(@StrawManList,1,LEN(@StrawManList)-1)
			SET @StrawManList = '<root><item><personid>'+@StrawManList+'</needby></item></root>'

			SET @StrawManList = REPLACE(@StrawManList,':','</personid><persontypeid>')
			SET @StrawManList = REPLACE(@StrawManList,'|','</persontypeid><qty>')
			SET @StrawManList = REPLACE(@StrawManList,'?','</qty><needby>')
			SET @StrawManList = REPLACE(@StrawManList,',','</needby></item><item><personid>')
			
			SELECT @PersonIdListLocalXML  = CONVERT(XML,@StrawManList)

			INSERT INTO @OpportunityPersonIdsWithTypeTable
			(PersonId ,
			PersonType ,
			Quantity,
			NeedBy)
			SELECT C.value('personid[1]','int') personid,
					C.value('persontypeid[1]','int') persontypeid,
					C.value('qty[1]','int') qty,
					C.value('needby[1]','DATETIME') needby
			FROM @PersonIdListLocalXML.nodes('/root/item') as T(C)

			INSERT INTO OpportunityPersons(OpportunityId,PersonId,OpportunityPersonTypeId,RelationTypeId,Quantity,NeedBy)
			SELECT @OpportunityId ,p.PersonId,p.PersonType,2,p.Quantity,p.NeedBy
			FROM @OpportunityPersonIdsWithTypeTable AS p 
		END 
		 
		
		-- End logging session
		EXEC dbo.SessionLogUnprepare
END

