CREATE PROCEDURE [dbo].[CloneBudgetMilestone]
(
	@MilestoneId        INT,
	@ProjectId			INT
)
AS
	SET NOCOUNT ON

	BEGIN TRY
	BEGIN TRAN  Tran_BudgetMilestoneClone

		DECLARE @MilestoneCloneId INT,
				@OldProjectId INT

		SELECT @OldProjectId=projectid from Milestone where MilestoneId=@MilestoneId

	-- Create a milestone record
	    INSERT INTO dbo.Milestone
	        (ProjectId, Description, Amount, StartDate, ProjectedDeliveryDate, IsHourlyAmount, MilestoneType, Discount, DiscountType, IsAmountAtMilestone)
	    SELECT @ProjectId,
	        SUBSTRING(m.Description + ' (cloned)', 1, 255),
			CASE 
	            WHEN m.ishourlyAmount=1 THEN null
	            ELSE p.Revenue
	        END,
	        m.StartDate,
	        m.ProjectedDeliveryDate,
	        m.IsHourlyAmount,
			1,
			m.Discount,
			m.DiscountType,
			1
	    FROM dbo.Milestone AS m
		join ProjectBudgetHistory P on m.MilestoneId=p.MilestoneId  
	    WHERE m.MilestoneId = @MilestoneId AND p.IsActive=1

		SET @MilestoneCloneId = SCOPE_IDENTITY()

	--insert personlist 
		INSERT INTO dbo.MilestonePerson
					(MilestoneId, PersonId)
		SELECT distinct @MilestoneCloneId, PBPE.PersonId
		FROM ProjectBudgetPersonEntry as PBPE
		WHERE PBPE.MilestoneId = @MilestoneId and PBPE.ProjectId= @OldProjectId

	--insert persos records into milestonepersonentry table

	    INSERT INTO dbo.MilestonePersonEntry (MilestonePersonId, StartDate, EndDate, Amount, HoursPerDay)
	    (
			SELECT MP.MilestonePersonId,
					PBPE.StartDate,
					PBPE.EndDate,
					PBPE.Amount,
					PBPE.HoursPerDay
			FROM ProjectBudgetPersonEntry PBPE
			JOIN MilestonePerson MP on MP.PersonId = PBPE.PersonId AND MP.MilestoneId = @MilestoneCloneId 
			WHERE PBPE.MilestoneId = @MilestoneId
		)

		IF EXISTS (SELECT 1 FROM Milestone WHERE MilestoneId = @MilestoneCloneId AND IsHourlyAmount = 0)
		BEGIN
			EXEC UpdateFixedFeeMilestoneDiscount @MilestoneId = @MilestoneCloneId, @UpdateMonthlyRevenues = 1
		END

	COMMIT TRAN Tran_BudgetMilestoneClone
	END TRY
	BEGIN CATCH
		ROLLBACK TRAN Tran_BudgetMilestoneClone
		
		DECLARE @ErrorMessage NVARCHAR(MAX)
		SET @ErrorMessage = ERROR_MESSAGE()

		RAISERROR(@ErrorMessage, 16, 1)
	END CATCH
