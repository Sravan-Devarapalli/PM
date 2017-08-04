-- =============================================
-- Author:		Sravan.D
-- Created date: 2017-06-21
-- Description:	Insert the records for budget and set the project for budget
-- =============================================

CREATE PROCEDURE [dbo].[UpdateProjectBudget] 
(
	@ProjectId          INT,
	@UserAlias          NVARCHAR(255),
	@RequestId			INT = 0,
	@ResetType			INT = 0, 
	@BudgetToDate		DATETIME = NULL
)
AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY
	BEGIN TRAN  ProjectBudgetUpdate

	-- @ResetType	
	-- 0 for •	Initial project Budget set i.e., at the time of moving project to active, RequestId = 0
	-- 1 for •	Change Order (Original Budget + Change Order Milestone)
	-- 2 for •	Change Order and General changes (Budget to date + Projected Remaining + CO Milestone)
	-- 3 for •	General Changes (Budget to date + Project Remaining)

	DECLARE @CurrentPMTime DATETIME,
			@RequestIdLocal INT,
			@ProjectIdLocal INT,
			@UpdatedBy INT,
			@BudgettoDateLocal DATETIME 
		
	 SELECT @UpdatedBy = PersonId FROM dbo.Person WHERE Alias = @UserAlias
	 SELECT @CurrentPMTime = dbo.GettingPMTime(GETUTCDATE()),
			@RequestIdLocal=@RequestId,
			@ProjectIdLocal	= @ProjectId,
			@BudgettoDateLocal= @BudgetToDate

	IF EXISTS(select 1 from milestone m  where m.ProjectId= @ProjectIdLocal and m.IsHourlyAmount = 0 ) and (@BudgettoDate IS NOT NULL)
	BEGIN
		SET @BudgettoDateLocal =EOMONTH(DATEADD(M, -1, @BudgetToDate))
	END

	IF(@ResetType = 0)
	BEGIN

		-- Clean up all the data related to previous budget
		EXEC [dbo].[CleanBudgetRecordsForNonActiveProject] @ProjectId = @ProjectId


		INSERT INTO ProjectBudgetPersonEntry(ProjectId, MilestoneId, PersonId, StartDate, EndDate, Amount, HoursPerDay)
		SELECT M.ProjectId,
			   M.MilestoneId,
			   MP.PersonId,
			   MPE.StartDate,
			   MPE.EndDate,
			   MPE.Amount,
			   MPE.HoursPerDay
		FROM MilestonePersonEntry MPE
		JOIN MilestonePerson MP on MP.MilestonePersonId = MPE.MilestonePersonId
		JOIN Milestone M on M.MilestoneId = MP.MilestoneId
		WHERE M.ProjectId = @ProjectIdLocal

		-- Insert Project Expenses
		INSERT INTO ProjectBudgetExpense(Id, ProjectId, MilestoneId, Name, Amount, Reimbursement,  StartDate, EndDate, ExpenseTypeId)
		SELECT PE.Id,
			   PE.ProjectId,
			   PE.MilestoneId,
			   PE.Name,
			   PE.ExpectedAmount,
			   PE.Reimbursement,
			   PE.StartDate,
			   PE.EndDate,
			   PE.ExpenseTypeId
		FROM ProjectExpense PE
		WHERE PE.ProjectId=@ProjectIdLocal
		
		-- Insert Project MonthlyExpense
		INSERT INTO ProjectBudgetMonthlyExpense(ExpenseId, StartDate, EndDate, Amount)
		SELECT PE.Id,
			   PME.StartDate,
			   PME.EndDate,
			   PME.EstimatedAmount
		FROM ProjectMonthlyExpense PME
		JOIN ProjectExpense PE ON PE.Id = PME.ExpenseId
		WHERE PE.ProjectId=@ProjectIdLocal

		--Insert FF Milestone Monthly Revenues
		INSERT INTO ProjectBudgetFFMilestoneMonthlyRevenue (ProjectId, MilestoneId, StartDate, EndDate, Amount)
		SELECT @ProjectIdLocal,
			   M.MilestoneId,
			   FFMR.StartDate,
			   FFMR.EndDate,
			   FFMR.Amount
		FROM FixedMilestoneMonthlyRevenue FFMR
		JOIN Milestone M on M.MilestoneId = FFMR.MilestoneId
		WHERE M.ProjectId = @ProjectIdLocal AND M.IsHourlyAmount = 0

	END

	IF(@ResetType = 1 OR @ResetType = 2)  -- CO Milestone
	BEGIN
		SELECT M.MilestoneId,
			   M.IsHourlyAmount
		INTO #COMilestone
		FROM Milestone M 
		LEFT JOIN ProjectBudgetHistory PBH on PBH.MilestoneId = M.MilestoneId and PBH.MilestoneId IS NOT NULL AND PBH.IsActive = 1
		WHERE M.ProjectId = @ProjectIdLocal  AND PBH.MilestoneId is  NULL AND M.MilestoneType = 2 
		
		INSERT INTO ProjectBudgetPersonEntry(ProjectId, MilestoneId, PersonId, StartDate, EndDate, Amount, HoursPerDay)
		SELECT @ProjectIdLocal,
			   COM.MilestoneId,
			   MP.PersonId,
			   MPE.StartDate,
			   MPE.EndDate,
			   MPE.Amount,
			   MPE.HoursPerDay
		FROM MilestonePersonEntry MPE
		JOIN MilestonePerson MP on MP.MilestonePersonId = MPE.MilestonePersonId
		JOIN #COMilestone COM on COM.MilestoneId = MP.MilestoneId
		

		-- Insert Project Expense
		INSERT INTO ProjectBudgetExpense(Id, ProjectId, MilestoneId, Name, Amount, Reimbursement,  StartDate, EndDate, ExpenseTypeId)
		SELECT PE.Id,
			   PE.ProjectId,
			   PE.MilestoneId,
			   PE.Name,
			   PE.ExpectedAmount,
			   PE.Reimbursement,
			   PE.StartDate,
			   PE.EndDate,
			   PE.ExpenseTypeId
		FROM ProjectExpense PE
		JOIN #COMilestone COM on COM.MilestoneId = PE.MilestoneId
	
		
		-- Insert Project MonthlyExpense
		INSERT INTO ProjectBudgetMonthlyExpense(ExpenseId, StartDate, EndDate, Amount)
		SELECT PE.Id,
			   PME.StartDate,
			   PME.EndDate,
			   PME.EstimatedAmount
		FROM ProjectMonthlyExpense PME
		JOIN ProjectExpense PE ON PE.Id = PME.ExpenseId
		JOIN #COMilestone COM on COM.MilestoneId = PE.MilestoneId

		--Insert FF Milestone Monthly Revenues
		INSERT INTO ProjectBudgetFFMilestoneMonthlyRevenue (ProjectId, MilestoneId, StartDate, EndDate, Amount)
		SELECT @ProjectIdLocal,
			   COM.MilestoneId,
			   FFMR.StartDate,
			   FFMR.EndDate,
			   FFMR.Amount
		FROM FixedMilestoneMonthlyRevenue FFMR
		JOIN #COMilestone COM on COM.MilestoneId = FFMR.MilestoneId
		WHERE COM.IsHourlyAmount = 0

		DROP TABLE #COMilestone
	END
	IF (@ResetType = 2 OR @ResetType = 3) -- Budget to date + Projected Remaining 
	BEGIN
		
		-- need to insert monthly FF revenue for budget 
		SELECT DISTINCT PBH.MilestoneId, 
						PBH.Revenue
		INTO #BudgetFFMilestone
		FROM ProjectBudgetHistory PBH
		WHERE PBH.ProjectId = @ProjectIdLocal and PBH.MilestoneId IS NOT NULL AND PBH.IsHourlyAmount = 0 and PBH.IsActive = 1

		-- Updates for budget to date
	
			SELECT BFM.MilestoneId,
				   MIN(PBPE.StartDate) as StartDate,
				   MAX(PBPE.EndDate) as EndDate,
				   BFM.Revenue
			INTO #ProjectBudgetFFSchedule 
			FROM ProjectBudgetPersonEntry PBPE
			JOIN #BudgetFFMilestone BFM ON BFM.MilestoneId = PBPE.MilestoneId
			GROUP BY BFM.MilestoneId, BFM.Revenue


			SELECT	PBPE.ProjectId,
					PBPE.MilestoneId,
					SUM(CONVERT(DECIMAL(4,2),CASE WHEN cal.DayOff = 0  THEN PBPE.HoursPerDay -- No Time-off and no company holiday
						WHEN cal.companydayoff = 1 OR ISNULL(cal.TimeoffHours,8) = 8 THEN 0 -- only company holiday OR person complete dayoff
						ELSE PBPE.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off
					END)) AS TotalWorkingHours,
					SUM( CASE WHEN cal.Date<=@BudgettoDateLocal THEN CONVERT(DECIMAL(4,2),CASE WHEN cal.DayOff = 0  THEN PBPE.HoursPerDay -- No Time-off and no company holiday
						WHEN cal.companydayoff = 1 OR ISNULL(cal.TimeoffHours,8) = 8 THEN 0 -- only company holiday OR person complete dayoff
						ELSE PBPE.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off
					END) ELSE 0 END) as WorkingHoursTillBudgetToDate
			INTO #FFMilestone
			FROM dbo.ProjectBudgetPersonEntry PBPE
			JOIN #BudgetFFMilestone FS on PBPE.MilestoneId = FS.MilestoneId
			JOIN dbo.PersonCalendarAuto AS cal ON cal.Date BETWEEN PBPE.StartDate AND PBPE.EndDate AND cal.PersonId = PBPE.PersonId 
			LEFT JOIN (select distinct milestoneid from dbo.ProjectBudgetFFMilestoneMonthlyRevenue WHERE ProjectId = @ProjectIdLocal) FMR on FS.MilestoneId=FMR.MilestoneId
			WHERE FMR.MilestoneId IS NULL
			GROUP BY PBPE.ProjectId,
					 PBPE.MilestoneId

			INSERT INTO ProjectBudgetFFMilestoneMonthlyRevenue (ProjectId, MilestoneId, StartDate, EndDate, Amount)
			SELECT @ProjectIdLocal,
				   FS.MilestoneId,
				   FS.StartDate,
				   @BudgettoDateLocal,
				   FS.Revenue*FM.WorkingHoursTillBudgetToDate/FM.TotalWorkingHours
			FROM #ProjectBudgetFFSchedule FS
			JOIN #FFMilestone FM on FM.MilestoneId = FS.MilestoneId
			WHERE @BudgettoDateLocal BETWEEN FS.StartDate AND FS.EndDate


		--END 
		--ELSE
		--BEGIN
			-- number of assigned working hours for FF milesone and revenue below the selected budget to date 

			-- Number of Hours in the partioned period
			SELECT	PBPE.ProjectId,
					PBPE.MilestoneId,
					SUM(CONVERT(DECIMAL(4,2),CASE WHEN cal.DayOff = 0  THEN PBPE.HoursPerDay -- No Time-off and no company holiday
						WHEN cal.companydayoff = 1 OR ISNULL(cal.TimeoffHours,8) = 8 THEN 0 -- only company holiday OR person complete dayoff
						ELSE PBPE.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off
					END)) AS TotalWorkingHours,
					SUM( CASE WHEN cal.Date<=@BudgettoDateLocal THEN CONVERT(DECIMAL(4,2),CASE WHEN cal.DayOff = 0  THEN PBPE.HoursPerDay -- No Time-off and no company holiday
						WHEN cal.companydayoff = 1 OR ISNULL(cal.TimeoffHours,8) = 8 THEN 0 -- only company holiday OR person complete dayoff
						ELSE PBPE.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off
					END) ELSE 0 END) as WorkingHoursTillBudgetToDate
			INTO #FFMilestoneResource
			FROM dbo.ProjectBudgetPersonEntry PBPE
			JOIN #BudgetFFMilestone BFM on PBPE.MilestoneId = BFM.MilestoneId
			JOIN dbo.ProjectBudgetFFMilestoneMonthlyRevenue PBFM ON PBFM.MilestoneId = BFM.MilestoneId AND @BudgettoDateLocal BETWEEN PBFM.StartDate AND PBFM.EndDate 
			JOIN dbo.PersonCalendarAuto AS cal ON cal.Date BETWEEN PBPE.StartDate AND PBPE.EndDate AND cal.PersonId = PBPE.PersonId 
			WHERE cal.Date BETWEEN PBFM.StartDate AND PBFM.EndDate
			GROUP BY PBPE.ProjectId,
					 PBPE.MilestoneId

			-- Budget the record which falls for Budget to date revenue with the proportionate revenue
			UPDATE PBFM
			SET PBFM.EndDate = @BudgettoDateLocal,
				PBFM.Amount = PBFM.Amount*FM.WorkingHoursTillBudgetToDate/FM.TotalWorkingHours
			FROM ProjectBudgetFFMilestoneMonthlyRevenue PBFM
			JOIN #FFMilestoneResource FM on PBFM.MilestoneId = FM.MilestoneId AND @BudgettoDateLocal BETWEEN PBFM.StartDate AND PBFM.EndDate 

			-- Delete the FF Budget Monthly records which are after Budget to date
			DELETE PBFM
			FROM ProjectBudgetFFMilestoneMonthlyRevenue PBFM
			JOIN #BudgetFFMilestone BFM ON PBFM.MilestoneId = BFM.MilestoneId
			WHERE PBFM.StartDate > @BudgettoDateLocal

		-- Updates For Projected Remaining do not include CO Milestone Which is not part of Origianal Budget (include general milestone even though not part of the Original Budget)

			SELECT M.MilestoneId,
				   M.IsHourlyAmount,
				   M.Amount,
				   M.StartDate,
				   M.ProjectedDeliveryDate as EndDate
			INTO #ProjectedMilestone
			FROM Milestone M 
			LEFT JOIN ProjectBudgetHistory PBH ON PBH.MilestoneId = M.MilestoneId and PBH.MilestoneId IS NOT NULL AND PBH.IsActive =1
			WHERE M.ProjectId = @ProjectIdLocal AND   (PBH.MilestoneId IS NOT NULL OR (PBH.MilestoneId IS NULL AND M.MilestoneType !=2))

			-- For FF Milestones Which Don't have monthly revenues
			SELECT	
					PM.MilestoneId,
					SUM(CONVERT(DECIMAL(4,2),CASE WHEN cal.DayOff = 0  THEN MPE.HoursPerDay -- No Time-off and no company holiday
						WHEN cal.companydayoff = 1 OR ISNULL(cal.TimeoffHours,8) = 8 THEN 0 -- only company holiday OR person complete dayoff
						ELSE MPE.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off
					END)) AS TotalWorkingHours,
					SUM( CASE WHEN cal.Date>@BudgettoDateLocal THEN CONVERT(DECIMAL(4,2),CASE WHEN cal.DayOff = 0  THEN MPE.HoursPerDay -- No Time-off and no company holiday
						WHEN cal.companydayoff = 1 OR ISNULL(cal.TimeoffHours,8) = 8 THEN 0 -- only company holiday OR person complete dayoff
						ELSE MPE.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off
					END) ELSE 0 END) as WorkingHoursPostBudgetToDate
			INTO #ProjectedFFMilestone
			FROM MilestonePersonEntry MPE
			JOIN MilestonePerson MP on MP.MilestonePersonId = MPE.MilestonePersonId
			JOIN #ProjectedMilestone PM ON PM.MilestoneId = MP.MilestoneId AND PM.IsHourlyAmount = 0
			JOIN dbo.PersonCalendarAuto AS cal ON cal.Date BETWEEN MPE.StartDate AND MPE.EndDate AND cal.PersonId = MP.PersonId 
			LEFT JOIN (select distinct milestoneid from dbo.FixedMilestoneMonthlyRevenue ) FMR on FMR.MilestoneId=PM.MilestoneId
			WHERE FMR.MilestoneId IS NULL
			GROUP BY PM.MilestoneId

			INSERT INTO ProjectBudgetFFMilestoneMonthlyRevenue (ProjectId, MilestoneId, StartDate, EndDate, Amount)
			SELECT @ProjectIdLocal,
				   PM.MilestoneId,
				   dateadd(day,1,@BudgettoDateLocal),
				   PM.EndDate,
				   PM.Amount*FM.WorkingHoursPostBudgetToDate/FM.TotalWorkingHours
			FROM #ProjectedFFMilestone FM
			JOIN #ProjectedMilestone PM on FM.MilestoneId = PM.MilestoneId
			WHERE @BudgettoDateLocal BETWEEN PM.StartDate AND PM.EndDate and dateadd(day,1,@BudgettoDateLocal) <= PM.EndDate
		
		-- For FF Milestones Which Do have monthly revenues
			SELECT	PM.MilestoneId,
					SUM(CONVERT(DECIMAL(4,2),CASE WHEN cal.DayOff = 0  THEN MPE.HoursPerDay -- No Time-off and no company holiday
						WHEN cal.companydayoff = 1 OR ISNULL(cal.TimeoffHours,8) = 8 THEN 0 -- only company holiday OR person complete dayoff
						ELSE MPE.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off
					END)) AS TotalWorkingHours,
					SUM( CASE WHEN cal.Date > @BudgettoDateLocal THEN CONVERT(DECIMAL(4,2),CASE WHEN cal.DayOff = 0  THEN MPE.HoursPerDay -- No Time-off and no company holiday
						WHEN cal.companydayoff = 1 OR ISNULL(cal.TimeoffHours,8) = 8 THEN 0 -- only company holiday OR person complete dayoff
						ELSE MPE.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off
					END) ELSE 0 END) as WorkingHoursPostBudgetToDate
			INTO #ProjectedFFMilestoneRevenue
			FROM MilestonePersonEntry MPE
			JOIN MilestonePerson MP on MP.MilestonePersonId = MPE.MilestonePersonId
			JOIN #ProjectedMilestone PM ON PM.MilestoneId = MP.MilestoneId AND PM.IsHourlyAmount = 0
			JOIN dbo.PersonCalendarAuto AS cal ON cal.Date BETWEEN MPE.StartDate AND MPE.EndDate AND cal.PersonId = MP.PersonId 
			JOIN  dbo.FixedMilestoneMonthlyRevenue  FMR on FMR.MilestoneId=PM.MilestoneId and @BudgettoDateLocal BETWEEN FMR.StartDate AND FMR.EndDate
			WHERE cal.Date BETWEEN FMR.StartDate AND FMR.EndDate
			GROUP BY PM.MilestoneId

			INSERT INTO ProjectBudgetFFMilestoneMonthlyRevenue (ProjectId, MilestoneId, StartDate, EndDate, Amount)
			SELECT @ProjectIdLocal,
				   FM.MilestoneId,
				   dateadd(day,1,@BudgettoDateLocal),
				   FMR.EndDate,
				   FMR.Amount*FM.WorkingHoursPostBudgetToDate/FM.TotalWorkingHours
			FROM #ProjectedFFMilestoneRevenue FM
			JOIN  dbo.FixedMilestoneMonthlyRevenue  FMR on FMR.MilestoneId=FM.MilestoneId and @BudgettoDateLocal BETWEEN FMR.StartDate AND FMR.EndDate
			WHERE dateadd(day,1,@BudgettoDateLocal)<=FMR.EndDate
			

		--Projected Remaining for the FF milestones included in Budget excluding the record which falls for Budget to date
			INSERT INTO ProjectBudgetFFMilestoneMonthlyRevenue (ProjectId, MilestoneId, StartDate, EndDate, Amount)
			SELECT @ProjectIdLocal,
				   PM.MilestoneId,
				   FFMR.StartDate,
				   FFMR.EndDate,
				   FFMR.Amount
			FROM FixedMilestoneMonthlyRevenue FFMR
			JOIN #ProjectedMilestone PM on PM.MilestoneId = FFMR.MilestoneId AND PM.IsHourlyAmount = 0
			WHERE FFMR.StartDate > @BudgettoDateLocal 
	


		-- Budget to date
		UPDATE PBPE
		SET PBPE.EndDate = @BudgettoDateLocal
		FROM ProjectBudgetPersonEntry PBPE
		WHERE PBPE.ProjectId = @ProjectIdLocal AND PBPE.StartDate<=@BudgettoDateLocal AND PBPE.EndDate > @BudgettoDateLocal

		DELETE PBPE
		FROM ProjectBudgetPersonEntry PBPE
		JOIN ProjectBudgetHistory PBH on PBH.MilestoneId IS NOT NULL AND PBH.MilestoneId = PBPE.MilestoneId AND PBH.IsActive = 1
		WHERE PBPE.ProjectId = @ProjectIdLocal AND PBPE.StartDate>@BudgettoDateLocal

		-- Projected Remaining
		INSERT INTO ProjectBudgetPersonEntry(ProjectId, MilestoneId, PersonId, StartDate, EndDate, Amount, HoursPerDay)
		SELECT @ProjectIdLocal,
			   PM.MilestoneId,
			   MP.PersonId,
			   CASE WHEN MPE.StartDate<= dateadd(day,1,@BudgettoDateLocal) THEN dateadd(day,1,@BudgettoDateLocal) ELSE MPE.StartDate END,
			   MPE.EndDate,
			   MPE.Amount,
			   MPE.HoursPerDay
		FROM MilestonePersonEntry MPE
		JOIN MilestonePerson MP on MP.MilestonePersonId = MPE.MilestonePersonId
		JOIN #ProjectedMilestone PM on PM.MilestoneId = MP.MilestoneId
		WHERE MPE.EndDate >= @BudgettoDateLocal


		DROP TABLE #BudgetFFMilestone
		DROP TABLE #ProjectBudgetFFSchedule
		DROP TABLE #FFMilestone
		DROP TABLE #FFMilestoneResource
		DROP TABLE #ProjectedMilestone
		DROP TABLE #ProjectedFFMilestone
		DROP TABLE #ProjectedFFMilestoneRevenue

	END


	EXEC dbo.SessionLogPrepare @UserLogin = @UserAlias

	UPDATE PBH
	   SET PBH.IsActive=CAST(0 as BIT)
	FROM ProjectBudgetHistory PBH
	WHERE PBH.ProjectId = @ProjectIdLocal AND PBH.IsActive = 1

	UPDATE MPE
	SET MPE.IsNewToBudget =CAST(0 as BIT)
	FROM MilestonePersonEntry MPE
	JOIN MilestonePerson MP on MP.MilestonePersonId =  MPE.MilestonePersonId
	JOIN Milestone M on M.MilestoneId = MP.MilestoneId
	WHERE M.ProjectId=@ProjectIdLocal AND MPE.IsNewToBudget=1
	
	EXEC dbo.SessionLogUnprepare
		
	exec dbo.ResetProjectBudget @ProjectId = @ProjectIdLocal , @UserAlias = @UserAlias

	EXEC dbo.SessionLogPrepare @UserLogin = @UserAlias
		   UPDATE P
	       SET P.Budget = F.Revenue,
		   p.BudgetSetDate=@CurrentPMTime
	       FROM dbo.Project P
	       JOIN ProjectBudgetHistory F ON F.ProjectId=P.ProjectId
	       WHERE P.ProjectId=@ProjectIdLocal AND F.IsActive = 1 AND F.MilestoneId IS NULL
	EXEC dbo.SessionLogUnprepare

	IF EXISTS(SELECT 1 FROM dbo.BudgetResetRequestHistory WHERE RequestId=@RequestId AND @ResetType != 0)
	BEGIN
		EXEC dbo.SessionLogPrepare @UserLogin = @UserAlias

		INSERT INTO dbo.BudgetResetApprovalHistory(RequestId,ApprovedBy,ApprovedDate,ResetType, BudgetToDate)
		VALUES (@RequestId,@UpdatedBy,@CurrentPMTime, @ResetType, @BudgetToDate)
		EXEC dbo.SessionLogUnprepare
	END
	
	COMMIT TRAN ProjectBudgetUpdate
	END TRY
	BEGIN CATCH
		ROLLBACK TRAN ProjectBudgetUpdate
		
		DECLARE @ErrorMessage NVARCHAR(MAX)
		SET @ErrorMessage = ERROR_MESSAGE()

		RAISERROR(@ErrorMessage, 16, 1)
	END CATCH

END
