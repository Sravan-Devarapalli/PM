CREATE PROCEDURE [dbo].[GetOpenTasksForUser]
(
	@UserAlias NVARCHAR(100)
)
AS
BEGIN
	   DECLARE  @UserId			INT,
				@ProjectIdLocal   NVARCHAR(MAX),
				@ActiveProjectIds NVARCHAR(MAX),
				@userTitleId  INT,
				@PracticeDirectorTitle INT,
				@CFOTitle INT,
				@IsPracticeDirector BIT = 0
			    
	   SELECT @userId = PersonId, @userTitleId=TitleId FROM dbo.Person WHERE Alias = @UserAlias

	   SELECT @PracticeDirectorTitle=TitleId from Title where Title='Practice Director'
	   SELECT @CFOTitle=TitleId from Title where Title='Chief Financial Officer'


	   IF EXISTS(SELECT 1 FROM persondivision WHERE PracticeDirectorId=@userId)
	   BEGIN
	     SET  @IsPracticeDirector = 1
	   END
	   ELSE 
	   BEGIN
		 SET @IsPracticeDirector = 0
	   END
	   
	    SELECT  @ProjectIdLocal = ISNULL(@ProjectIdLocal,'') + ',' + CONVERT(VARCHAR,P.ProjectId)
		FROM	dbo.Project AS P
		WHERE	((P.StartDate IS NOT NULL AND P.EndDate IS NOT NULL) OR P.ProjectStatusId=8)  
			AND (P.ExecutiveInChargeId = @UserId OR P.EngagementManagerId = @UserId OR P.ProjectManagerId=@UserId )
			AND P.IsAllowedToShow = 1 AND P.ProjectId NOT IN (SELECT ProjectId FROM dbo.DefaultMilestoneSetting)
			AND P.ProjectStatusId IN (2,8) and P.startdate>='01/01/2014' -- projected and At-Risk Projects

		SELECT  @ActiveProjectIds = ISNULL(@ActiveProjectIds,'') + ',' + CONVERT(VARCHAR,P.ProjectId)
		FROM	dbo.Project AS P
		WHERE	((P.StartDate IS NOT NULL AND P.EndDate IS NOT NULL) OR P.ProjectStatusId=8)  
			AND (P.ExecutiveInChargeId = @UserId OR P.EngagementManagerId = @UserId OR P.ProjectManagerId=@UserId )
			AND P.IsAllowedToShow = 1 AND P.ProjectId NOT IN (SELECT ProjectId FROM dbo.DefaultMilestoneSetting)
			AND P.ProjectStatusId = 3 and P.startdate>='01/01/2014' -- Active Projects
		
	   
		
			SELECT M.ProjectId, M.TierOneStatus, M.TierTwoStatus 
			INTO #MarginExceptionPending
			FROM MarginExceptionRequest M
			JOIN Project P ON P.ProjectId= M.ProjectId
			JOIN ProjectDivision PD ON PD.DivisionId = P.DivisionId
			JOIN PersonDivision PerD ON PerD.DivisionName = PD.DivisionName 
			WHERE (M.TierOneStatus=1 AND PerD.DivisionOwnerId = @UserId)
				OR (M.TierOneStatus =2 AND M.TierTwoStatus=1 AND @userTitleId=@CFOTitle) 
		

		IF EXISTS(SELECT 1 FROM PersonDivision WHERE DivisionOwnerId = @UserId)
		BEGIN
			SELECT P.ProjectId,
			   P.ProjectNumber,
			   P.Name,
			   P.ProjectStatusId,
			   p.BusinessTypeId,
			   NULL as Revenue,
			   NULL AS RevenueNet,
			   NULL as GrossMargin,
			   M.TierOneStatus,
			   M.TierTwoStatus,
			   50 as MarginGoal,
			   NULL AS MarginThreshold,
			   NULL as ExceptionRevenue
		FROM PROJECT p
		INNER JOIN #MarginExceptionPending M ON M.Projectid=P.ProjectId
		END
		ELSE
		BEGIN
			
			CREATE TABLE #Financials
			(
			   ProjectId INT,
			   Revenue DECIMAL,
			   RevenueNet DECIMAL,
			   GrossMargin DECIMAL
			)
			INSERT INTO #Financials(ProjectId,Revenue, RevenueNet, GrossMargin)
			EXEC dbo.FinancialsGetByProjectIds @ProjectId=@ProjectIdLocal

			INSERT INTO #Financials(ProjectId,Revenue, RevenueNet, GrossMargin)
			EXEC dbo.GetEACFinancialsByProjectIds @ProjectId=@ActiveProjectIds

			CREATE CLUSTERED INDEX C_ixFinancialsTempTable ON #Financials(ProjectId)

			SELECT P.ProjectId,
				   P.ProjectNumber,
				   P.Name,
				   P.ProjectStatusId,
				   p.BusinessTypeId,
				   F.Revenue,
				   F.RevenueNet,
				   F.GrossMargin,
				   0 as TierOneStatus,
				   0 as TierTwoStatus,
				   CASE WHEN P.ExceptionMargin IS NOT NULL THEN P.ExceptionMargin 
						WHEN C.MarginGoal IS NOT NULL THEN C.MarginGoal
						ELSE 50 END AS MarginGoal,
				   ME.MarginGoal AS MarginThreshold,
				   ME.Revenue as ExceptionRevenue
			FROM Project P
			INNER JOIN #Financials F ON P.ProjectId=F.ProjectId
			LEFT JOIN ClientMarginGoal C ON C.ClientId=P.ClientId AND (C.StartDate<= P.StartDate AND C.EndDate>=P.StartDate)
			LEFT JOIN MarginException ME ON ME.StartDate<=P.StartDate AND ME.EndDate>=P.StartDate AND ME.ApprovalLevelId=1
			UNION ALL
			SELECT P.ProjectId,
				   P.ProjectNumber,
				   P.Name,
				   P.ProjectStatusId,
				   p.BusinessTypeId,
				   NULL as Revenue,
				   NULL AS RevenueNet,
				   NULL as GrossMargin,
				   M.TierOneStatus,
				   M.TierTwoStatus,
				   CASE WHEN P.ExceptionMargin IS NOT NULL THEN P.ExceptionMargin 
						ELSE 50 END AS MarginGoal,
				   NULL AS MarginThreshold,
				   NULL as ExceptionRevenue
			FROM PROJECT p
			INNER JOIN #MarginExceptionPending M ON M.Projectid=P.ProjectId
			order by P.ProjectNumber DESC

			DROP TABLE #Financials
		END


		--WHERE P.ProjectStatusId = 8 OR M.TierOneStatus = 1 OR (M.TierTwoStatus = 1 AND M.TierOneStatus = 2)

		
		DROP TABLE #MarginExceptionPending
END

