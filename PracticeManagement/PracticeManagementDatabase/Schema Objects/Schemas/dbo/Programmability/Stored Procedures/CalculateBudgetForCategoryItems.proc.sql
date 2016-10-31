﻿CREATE PROCEDURE [dbo].[CalculateBudgetForCategoryItems]
(
	@CategoryTypeId			INT,
	@StartDate				DATETIME,
	@EndDate				DATETIME,
	@ItemIds				NVARCHAR(MAX) = NULL,
	@PracticeIds			NVARCHAR(MAX) = NULL,
	@ShowProjected			BIT = 0,
	@ShowCompleted			BIT = 0,
	@ShowActive				BIT = 0,
	@showInternal			BIT = 0,
	@ShowExperimental		BIT = 0,
	@ShowProposed			BIT = 0,
	@ShowInactive			BIT = 0,
	@ShowAtRisk				BIT = 0,
	@ExcludeInternalPractices BIT = 0
)
AS
BEGIN

	DECLARE  @CategoryTypeIdLocal		INT,
			 @StartDateLocal			DATETIME,
			 @EndDateLocal				DATETIME,
			 @ItemIdsLocal				NVARCHAR(MAX),
			 @PracticeIdsLocal			NVARCHAR(MAX),
			 @ShowProjectedLocal		BIT,
			 @ShowCompletedLocal		BIT,
			 @ShowActiveLocal			BIT,
			 @ShowAtRiskLocal			BIT,
			 @showInternalLocal			BIT,
			 @ShowExperimentalLocal		BIT,
			 @ShowProposedLocal			BIT,
			 @ShowInactiveLocal			BIT,
			 @ExcludeInternalPracticesLocal BIT,
			 @FutureDateLocal			DATETIME

	SELECT 
	 @CategoryTypeIdLocal			=	@CategoryTypeId,
	 @StartDateLocal				=	@StartDate,
	 @EndDateLocal					=	@EndDate,
	 @ItemIdsLocal					=	@ItemIds,
	 @PracticeIdsLocal				=	@PracticeIds,
	 @ShowProjectedLocal			=	@ShowProjected,
	 @ShowCompletedLocal			=	@ShowCompleted,
	 @ShowActiveLocal				=	@ShowActive,
	 @ShowAtRiskLocal				=	@ShowAtRisk,
	 @showInternalLocal				=	@showInternal,
	 @ShowExperimentalLocal			=	@ShowExperimental,
	 @ShowProposedLocal				=   @ShowProposed,
	 @ShowInactiveLocal				=	@ShowInactive,
	 @ExcludeInternalPracticesLocal	=	@ExcludeInternalPractices,
	 @FutureDateLocal				=	dbo.GetFutureDate()

	IF(@CategoryTypeIdLocal = 1) -- Client Director
	BEGIN
	
		;With ClientDirectors
		AS
		(
			SELECT 
				P.PersonId,
				P.LastName,
				ISNULL(P.PreferredFirstName,P.FirstName) AS FirstName,
				C.MonthStartDate,
				CIB.Amount
			FROM dbo.Person P
			INNER JOIN dbo.v_PersonHistory PH ON PH.PersonId = P.PersonId 
			JOIN dbo.Calendar C ON C.Date BETWEEN PH.HireDate AND ISNULL(PH.TerminationDate,@FutureDateLocal) 
									AND C.Date  BETWEEN @StartDateLocal AND	 @EndDateLocal
			LEFT JOIN dbo.PersonStatusHistory PSH 
				ON PSH.PersonId = P.PersonId AND PSH.PersonStatusId IN (1,5) -- ACTIVE And Terminated Pending
				  AND C.Date >= PSH.StartDate AND (C.Date <= PSH.EndDate OR PSH.EndDate IS NULL)
			LEFT JOIN dbo.aspnet_Users U ON P.Alias = U.UserName
			LEFT JOIN dbo.aspnet_UsersRolesHistory  UIR
			ON UIR.UserId = U.UserId  AND C.Date >= UIR.StartDate AND (C.Date <= UIR.EndDate OR UIR.EndDate IS NULL)
			LEFT JOIN dbo.aspnet_Roles UR ON UIR.RoleId = UR.RoleId AND UR.RoleName='Client Director'
			LEFT JOIN dbo.Project Proj ON proj.ExecutiveInChargeId = P.PersonId AND C.Date BETWEEN Proj.StartDate 
					AND ISNULL(Proj.EndDate,@FutureDateLocal)
			LEFT JOIN dbo.CategoryItemBudget CIB ON CIB.CategoryTypeId = @CategoryTypeId 
							AND CIB.MonthStartDate  BETWEEN @StartDateLocal AND @EndDateLocal 
							AND MONTH(CIB.MonthStartDate) = MONTH(C.Date)
								AND CIB.ItemId = P.PersonId 
			WHERE (UR.RoleId IS NOT NULL OR Proj.ProjectId IS NOT NULL)
				  AND (PSH.PersonStatusId IN (1,5) -- ACTIVE And Terminated Pending
				   OR (PSH.PersonId IS NULL AND Proj.ProjectId IS NOT NULL ))
			GROUP BY P.PersonId,
					 P.LastName,
					 ISNULL(P.PreferredFirstName,P.FirstName),
					 CIB.Amount,
					 C.MonthStartDate
			
		),
		CTEMilestonePersonSchedule
		AS
		(
			SELECT  m.[MilestoneId],
				mp.PersonId,
				m.ProjectId,
				m.IsHourlyAmount,
				mpe.MilestonePersonId,
				dbo.PersonProjectedHoursPerDay(cal.DayOff,cal.companydayoff,cal.TimeoffHours,mpe.HoursPerDay) AS HoursPerDay,
				cal.Date,
				mpe.StartDate AS EntryStartDate,
				mpe.Amount
			FROM dbo.[Milestone] AS m
			INNER JOIN dbo.MilestonePerson AS mp ON m.[MilestoneId] = mp.[MilestoneId]
			INNER JOIN dbo.MilestonePersonEntry AS mpe ON mp.MilestonePersonId = mpe.MilestonePersonId	
			INNER JOIN dbo.PersonCalendarAuto AS cal ON (cal.Date BETWEEN mpe.Startdate AND mpe.EndDate AND cal.PersonId = mp.PersonId)
		),
		CTEFinancialsRetroSpective
		AS
		(	  	  
			SELECT r.ProjectId,
				r.MilestoneId,
				r.Date,
				CASE
					WHEN r.IsHourlyAmount = 1 OR s.HoursPerDay = 0
					THEN ISNULL(m.Amount*m.HoursPerDay, 0)
					ELSE ISNULL(r.MilestoneDailyAmount * m.HoursPerDay / s.HoursPerDay, r.MilestoneDailyAmount)
				END AS PersonMilestoneDailyAmount
			FROM  (			  
						SELECT -- Milestones with a fixed amount
							m.MilestoneId,
							m.ProjectId,
							p.ProjectStatusId,
							prac.PracticeId,
							prac.[IsCompanyInternal],
							cal.Date,
							m.IsHourlyAmount,
							ISNULL((m.Amount / (SELECT  SUM(HoursPerDay)
												FROM CTEMilestonePersonSchedule m1 WHERE  m1.MileStoneId = m.MilestoneId
										)) * ISNULL(d.HoursPerDay, 0),
									(CASE (DATEDIFF(dd, m.StartDate, m.ProjectedDeliveryDate) + 1)
										WHEN 0 THEN 0
										ELSE m.Amount / (DATEDIFF(dd, m.StartDate, m.ProjectedDeliveryDate) + 1)
									END)) AS MilestoneDailyAmount,
							p.Discount
						FROM dbo.Milestone AS m
						INNER JOIN dbo.Calendar AS cal ON cal.Date BETWEEN m.StartDate AND m.ProjectedDeliveryDate
						INNER JOIN dbo.Project AS p ON m.ProjectId = p.ProjectId
						INNER JOIN dbo.Practice AS prac ON p.PracticeId = prac.PracticeId
						LEFT JOIN (
									SELECT  ps1.[MilestoneId],
									SUM(ps1.HoursPerDay )   HoursPerDay,
									ps1.Date
									FROM  CTEMilestonePersonSchedule ps1
									GROUP BY ps1.Date,ps1.MilestoneId
									) d ON d.date = cal.Date and m.MilestoneId = d.MileStoneId
						WHERE m.IsHourlyAmount = 0
				 
						UNION ALL

						SELECT ps2.[MilestoneId],
								ps2.ProjectId,
								p.ProjectStatusId,
								prac.PracticeId,
								prac.[IsCompanyInternal],
								ps2.Date,
								ps2.IsHourlyAmount,
								ISNULL(SUM(ps2.Amount *( ps2.HoursPerDay )),0) MilestoneDailyAmount,
								MAX(p.Discount) AS Discount
							FROM CTEMilestonePersonSchedule ps2
								INNER JOIN dbo.Project AS p ON ps2.ProjectId = p.ProjectId
								INNER JOIN dbo.Practice AS prac ON p.PracticeId = prac.PracticeId
							WHERE ps2.IsHourlyAmount = 1
						GROUP BY ps2.MilestoneId, 
								ps2.ProjectId, 
								p.ProjectStatusId,
								prac.PracticeId,
								prac.[IsCompanyInternal],
								ps2.Date, ps2.IsHourlyAmount, prac.PracticeManagerId
				) AS r
				-- Linking to persons
				LEFT JOIN  CTEMilestonePersonSchedule m ON m.MilestoneId = r.MilestoneId AND m.Date = r.Date
				LEFT JOIN (
								SELECT  ps3.[MilestoneId],
										SUM(ps3.HoursPerDay )   HoursPerDay,
										ps3.Date
								FROM  CTEMilestonePersonSchedule ps3
								GROUP BY ps3.Date,ps3.MilestoneId
							) AS s  ON s.Date = r.Date AND s.MilestoneId = r.MilestoneId 
				-- Salary
			WHERE r.Date  BETWEEN @StartDateLocal AND	 @EndDateLocal
					AND (    ( @ShowProjectedLocal = 1 AND r.ProjectStatusId = 2 )
							OR ( @ShowActiveLocal = 1 AND r.ProjectStatusId = 3 )
							OR ( @ShowCompletedLocal = 1 AND r.ProjectStatusId = 4 )
							OR ( @showInternalLocal = 1 AND r.ProjectStatusId = 6 ) -- Internal
							OR ( @ShowExperimentalLocal = 1 AND r.ProjectStatusId = 5 )
							OR ( @ShowProposedLocal = 1 AND r.ProjectStatusId = 7 ) -- Proposed
							OR ( @ShowInactiveLocal = 1 AND r.ProjectStatusId = 1 ) -- Inactive
							OR ( @ShowAtRiskLocal = 1 AND r.ProjectStatusId = 8 )
						)
					AND ( @PracticeIdsLocal IS NULL OR r.PracticeId IN (SELECT ResultId FROM [dbo].[ConvertStringListIntoTable](@PracticeIdsLocal))OR r.PracticeId IS NULL )
					AND (ISNULL(r.IsCompanyInternal, 0) = 0 AND @ExcludeInternalPracticesLocal  = 1 OR @ExcludeInternalPracticesLocal = 0)
			GROUP BY r.Date, r.ProjectId, r.MilestoneId, r.MilestoneDailyAmount, r.Discount, 
					m.Amount, s.HoursPerDay,r.IsHourlyAmount, m.HoursPerDay, m.PersonId,
					m.MilestonePersonId, m.EntryStartDate
		),
		ProjectExpensesMonthly
		AS
		(

			SELECT pexp.ProjectId,
				CONVERT(DECIMAL(18,2),SUM(pexp.Amount/((DATEDIFF(dd,pexp.StartDate,pexp.EndDate)+1)))) Expense,
				CONVERT(DECIMAL(18,2),SUM(pexp.Reimbursement*0.01*pexp.Amount /((DATEDIFF(dd,pexp.StartDate,pexp.EndDate)+1)))) Reimbursement,
				C.MonthStartDate AS FinancialDate
			FROM dbo.ProjectExpense as pexp
			JOIN dbo.Calendar c ON c.Date BETWEEN pexp.StartDate AND pexp.EndDate
			WHERE c.Date BETWEEN @StartDateLocal AND @EndDateLocal
			GROUP BY pexp.ProjectId, C.MonthStartDate
		),
		ProjectMonthlyFinancials
		AS
		(
		
			SELECT  ISNULL(Fin.ProjectId,PEM.ProjectId) ProjectId,
					ISNULL(Fin.MonthStartDate,PEM.FinancialDate) MonthStartDate,
					ISNULL(Fin.Revenue,0)+ISNULL(Reimbursement,0)-ISNULL(Expense,0) Revenue
			FROM 
			(
				SELECT	F.ProjectId,
						C.MonthStartDate AS MonthStartDate,
						SUM(f.PersonMilestoneDailyAmount) AS Revenue
				FROM  CTEFinancialsRetroSpective F
				INNER JOIN dbo.Calendar C ON C.Date = F.Date
				WHERE F.Date  BETWEEN @StartDateLocal AND	 @EndDateLocal
				GROUP BY F.ProjectId, C.MonthStartDate
			) Fin
			FULL JOIN ProjectExpensesMonthly PEM
			ON PEM.ProjectId = FIN.ProjectId AND PEM.FinancialDate = Fin.MonthStartDate
		)

		SELECT CD.PersonId,
				CD.LastName,
				CD.FirstName,
				CD.MonthStartDate,
				CD.Amount,
				B.Revenue
		FROM ClientDirectors CD
		LEFT JOIN 
		(
		SELECT P.ExecutiveInChargeId AS DirectorId,
			   F.MonthStartDate,
			   SUM(Revenue) AS Revenue
		FROM dbo.Project P
		JOIN ProjectMonthlyFinancials F
				ON F.ProjectId = P.ProjectId
		WHERE P.ExecutiveInChargeId IS NOT NULL
		GROUP BY P.ExecutiveInChargeId,MonthStartDate
		) B
		ON CD.PersonId = B.DirectorId AND B.MonthStartDate = CD.MonthStartDate
		ORDER BY CD.LastName, CD.FirstName
	END
	ELSE IF (@CategoryTypeIdLocal = 3) --Business Development Manager
	BEGIN
		;With BusinessDevelopmentDManagers
		AS
		(
			SELECT 
				P.PersonId,
				P.LastName,
				ISNULL(P.PreferredFirstName,P.FirstName) AS FirstName,
				C.MonthStartDate MonthStartDate,
				CIB.Amount
			FROM dbo.Person P
			INNER JOIN dbo.v_PersonHistory PH ON PH.PersonId = P.PersonId 
			JOIN dbo.Calendar C ON C.Date BETWEEN PH.HireDate AND ISNULL(PH.TerminationDate,@FutureDateLocal) 
									AND C.Date  BETWEEN @StartDateLocal AND	 @EndDateLocal
			LEFT JOIN dbo.PersonStatusHistory PSH 
				ON PSH.PersonId = P.PersonId AND PSH.PersonStatusId IN (1,5) -- ACTIVE And Terminated Pending
				  AND C.Date >= PSH.StartDate AND (C.Date <= PSH.EndDate OR PSH.EndDate IS NULL)
			LEFT JOIN dbo.aspnet_Users U ON P.Alias = U.UserName
			LEFT JOIN dbo.aspnet_UsersRolesHistory  UIR
			ON UIR.UserId = U.UserId  AND C.Date >= UIR.StartDate AND (C.Date <= UIR.EndDate OR UIR.EndDate IS NULL)
			LEFT JOIN dbo.aspnet_Roles UR ON UIR.RoleId = UR.RoleId AND UR.RoleName='Salesperson'
			LEFT JOIN dbo.Project Proj ON C.Date BETWEEN Proj.StartDate  AND ISNULL(Proj.EndDate,@FutureDateLocal)
			LEFT JOIN dbo.CategoryItemBudget CIB ON CIB.CategoryTypeId = @CategoryTypeId 
							AND CIB.MonthStartDate  BETWEEN @StartDateLocal AND	 @EndDateLocal
							AND MONTH(CIB.MonthStartDate) = MONTH(C.Date)
								AND CIB.ItemId = P.PersonId 
			WHERE (UR.RoleId IS NOT NULL OR Proj.SalesPersonId = P.PersonId)
				  AND (PSH.PersonStatusId IS NOT NULL OR(Proj.SalesPersonId IS NOT NULL AND Proj.ProjectId IS NOT NULL))
				  AND (P.PersonId IN (SELECT * From [dbo].[ConvertStringListIntoTable](@ItemIdsLocal) )
						OR @ItemIdsLocal IS NULL)
			GROUP BY P.PersonId,
					 P.LastName,
					 ISNULL(P.PreferredFirstName,P.FirstName),
					 CIB.Amount,
					 C.MonthStartDate

		),
		CTEMilestonePersonSchedule
		AS
		(
		  SELECT  m.[MilestoneId],
				mp.PersonId,
				m.ProjectId,
				m.IsHourlyAmount,
				mpe.MilestonePersonId,
				dbo.PersonProjectedHoursPerDay(cal.DayOff,cal.companydayoff,cal.TimeoffHours,mpe.HoursPerDay) AS HoursPerDay,
				cal.Date,
				mpe.StartDate AS EntryStartDate,
				mpe.Amount
		   FROM dbo.[Milestone] AS m
		   INNER JOIN dbo.MilestonePerson AS mp ON m.[MilestoneId] = mp.[MilestoneId]
		   INNER JOIN dbo.MilestonePersonEntry AS mpe ON mp.MilestonePersonId = mpe.MilestonePersonId			
		   INNER JOIN dbo.PersonCalendarAuto AS cal ON (cal.Date BETWEEN mpe.Startdate AND mpe.EndDate AND cal.PersonId = mp.PersonId)
		),
		CTEFinancialsRetroSpective
		AS
		(	  	  
			  SELECT r.ProjectId,
					 r.MilestoneId,
					 r.Date,
					 CASE
						WHEN r.IsHourlyAmount = 1 OR s.HoursPerDay = 0
						THEN ISNULL(m.Amount*m.HoursPerDay, 0)
						ELSE ISNULL(r.MilestoneDailyAmount * m.HoursPerDay / s.HoursPerDay, r.MilestoneDailyAmount)
					 END AS PersonMilestoneDailyAmount
			  FROM  (			  
						  SELECT -- Milestones with a fixed amount
							   m.MilestoneId,
							   m.ProjectId,
							   P.ProjectStatusId,
							   prac.PracticeId,
							   prac.[IsCompanyInternal],
							   cal.Date,
							   m.IsHourlyAmount,
							   ISNULL((m.Amount / (SELECT  SUM(HoursPerDay)
												   FROM CTEMilestonePersonSchedule m1 WHERE  m1.MileStoneId = m.MilestoneId
											)) * ISNULL(d.HoursPerDay, 0),
									  (CASE (DATEDIFF(dd, m.StartDate, m.ProjectedDeliveryDate) + 1)
										   WHEN 0 THEN 0
										   ELSE m.Amount / (DATEDIFF(dd, m.StartDate, m.ProjectedDeliveryDate) + 1)
									   END)) AS MilestoneDailyAmount,
							   p.Discount
						  FROM dbo.Milestone AS m
							   INNER JOIN dbo.Calendar AS cal ON cal.Date BETWEEN m.StartDate AND m.ProjectedDeliveryDate
							   INNER JOIN dbo.Project AS p ON m.ProjectId = p.ProjectId
							   INNER JOIN dbo.Practice AS prac ON p.PracticeId = prac.PracticeId
							   LEFT JOIN (
										   SELECT  ps1.[MilestoneId],
											SUM(ps1.HoursPerDay )   HoursPerDay,
											ps1.Date
											FROM  CTEMilestonePersonSchedule ps1
										GROUP BY ps1.Date,ps1.MilestoneId
									   ) d ON d.date = cal.Date and m.MilestoneId = d.MileStoneId
						  WHERE m.IsHourlyAmount = 0
				 
						  UNION ALL

						  SELECT ps2.[MilestoneId],
								 ps2.ProjectId,
								 P.ProjectStatusId,
								 prac.PracticeId,
								 prac.[IsCompanyInternal],
								 ps2.Date,
							    ps2.IsHourlyAmount,
							    ISNULL(SUM(ps2.Amount *( ps2.HoursPerDay )),0) MilestoneDailyAmount,
							    MAX(p.Discount) AS Discount
						  FROM CTEMilestonePersonSchedule ps2
							   INNER JOIN dbo.Project AS p ON ps2.ProjectId = p.ProjectId
							   INNER JOIN dbo.Practice AS prac ON p.PracticeId = prac.PracticeId
						 WHERE ps2.IsHourlyAmount = 1
						 GROUP BY ps2.MilestoneId, 
								ps2.ProjectId, 
								p.ProjectStatusId,
								prac.PracticeId,
								prac.[IsCompanyInternal],
								ps2.Date, ps2.IsHourlyAmount, prac.PracticeManagerId
					) AS r
				   -- Linking to persons
				   LEFT JOIN  CTEMilestonePersonSchedule m ON m.MilestoneId = r.MilestoneId AND m.Date = r.Date
				   LEFT JOIN (
								SELECT  ps3.[MilestoneId],
										SUM(ps3.HoursPerDay )   HoursPerDay,
										ps3.Date
								FROM  CTEMilestonePersonSchedule ps3
								GROUP BY ps3.Date,ps3.MilestoneId
							  ) AS s  ON s.Date = r.Date AND s.MilestoneId = r.MilestoneId 
			WHERE r.Date  BETWEEN @StartDateLocal AND	 @EndDateLocal
					AND (    ( @ShowProjectedLocal = 1 AND r.ProjectStatusId = 2 )
							OR ( @ShowActiveLocal = 1 AND r.ProjectStatusId = 3 )
							OR ( @ShowCompletedLocal = 1 AND r.ProjectStatusId = 4 )
							OR ( @showInternalLocal = 1 AND r.ProjectStatusId = 6 ) -- Internal
							OR ( @ShowExperimentalLocal = 1 AND r.ProjectStatusId = 5 )
							OR ( @ShowProposedLocal = 1 AND r.ProjectStatusId = 7 ) -- Proposed
							OR ( @ShowInactiveLocal = 1 AND r.ProjectStatusId = 1 ) -- Inactive
							OR ( @ShowAtRiskLocal = 1 AND r.ProjectStatusId = 8 )
						)
					AND ( @PracticeIdsLocal IS NULL OR r.PracticeId IN (SELECT ResultId FROM [dbo].[ConvertStringListIntoTable](@PracticeIdsLocal)) OR r.PracticeId IS NULL )
					AND (ISNULL(r.IsCompanyInternal, 0) = 0 AND @ExcludeInternalPracticesLocal  = 1 OR @ExcludeInternalPracticesLocal = 0)
			GROUP BY r.Date, r.ProjectId, r.MilestoneId, r.MilestoneDailyAmount, r.Discount,
					 m.Amount, s.HoursPerDay,r.IsHourlyAmount, m.HoursPerDay, 
					 m.PersonId,m.MilestonePersonId, m.EntryStartDate
		) ,
		ProjectExpensesMonthly
		AS
		(
			SELECT pexp.ProjectId,
				CONVERT(DECIMAL(18,2),SUM(pexp.Amount/((DATEDIFF(dd,pexp.StartDate,pexp.EndDate)+1)))) Expense,
				CONVERT(DECIMAL(18,2),SUM(pexp.Reimbursement*0.01*pexp.Amount /((DATEDIFF(dd,pexp.StartDate,pexp.EndDate)+1)))) Reimbursement,
				C.MonthStartDate AS FinancialDate
			FROM dbo.ProjectExpense as pexp
			JOIN dbo.Calendar c ON c.Date BETWEEN pexp.StartDate AND pexp.EndDate
			WHERE c.Date BETWEEN @StartDateLocal AND @EndDateLocal
			GROUP BY pexp.ProjectId, C.MonthStartDate
		),
		ProjectFinancialsMonthly
		AS
		(
		SELECT ISNULL(Fin.ProjectId,PEM.ProjectId) ProjectId,
				ISNULL(Fin.MonthStartDate,PEM.FinancialDate) MonthStartDate,
				ISNULL(Fin.Revenue,0)+ISNULL(Reimbursement,0)-ISNULL(Expense,0) Revenue
		FROM 
		(
			SELECT	F.ProjectId,
					C.MonthStartDate AS MonthStartDate,
					SUM(f.PersonMilestoneDailyAmount) AS Revenue
			 FROM CTEFinancialsRetroSpective F
			 INNER JOIN dbo.Calendar C ON C.Date = F.Date
			WHERE  F.Date  BETWEEN @StartDateLocal AND	 @EndDateLocal  
			GROUP BY F.ProjectId, C.MonthStartDate
		) Fin
		FULL JOIN ProjectExpensesMonthly PEM
		ON PEM.ProjectId = FIN.ProjectId AND PEM.FinancialDate = Fin.MonthStartDate
		)

		SELECT BDM.PersonId,
			   BDM.LastName,
			   BDM.FirstName,
			   BDM.MonthStartDate,
			   BDM.Amount,
			   B.Revenue
		FROM BusinessDevelopmentDManagers BDM
		LEFT JOIN 
		(
		SELECT P.SalesPersonId AS PersonId,
	       MonthStartDate,
	       SUM(f.Revenue) AS Revenue
		FROM  dbo.Project P 
		INNER JOIN ProjectFinancialsMonthly F ON F.ProjectId = P.ProjectId
		GROUP BY P.SalesPersonId,MonthStartDate
		) B 	ON BDM.PersonId = B.PersonId AND BDM.MonthStartDate = B.MonthStartDate
		
		
		ORDER BY BDM.LastName, BDM.FirstName
					
	END
	ELSE -- Practice Area
	BEGIN
		;With PracticeAreas
		AS
		(
			SELECT  P.PracticeId,
					p.Name,
					C.MonthStartDate MonthStartDate,
					CIB.Amount
			FROM dbo.Practice P
			JOIN dbo.Calendar C ON   C.Date  BETWEEN @StartDateLocal AND	 @EndDateLocal
			LEFT JOIN dbo.PracticeStatusHistory PSH ON P.PracticeId = PSH.PracticeId AND Psh.IsActive = 1
			LEFT JOIN dbo.Project proj
			ON Proj.PracticeId = P.PracticeId  
					AND C.Date BETWEEN Proj.StartDate  AND ISNULL(Proj.EndDate,@FutureDateLocal)
			LEFT JOIN dbo.CategoryItemBudget CIB 
			ON CIB.ItemId = P.PracticeId AND CIB.CategoryTypeId = @CategoryTypeId
				AND CIB.MonthStartDate BETWEEN @StartDateLocal AND	 @EndDateLocal
				AND MONTH(CIB.MonthStartDate) = MONTH(C.Date)
			WHERE (PSH.PracticeId IS NOT NULL OR Proj.ProjectId IS NOT NULL)
					AND ( @PracticeIdsLocal IS NULL OR P.PracticeId IN (SELECT ResultId FROM [dbo].[ConvertStringListIntoTable](@PracticeIdsLocal)))
					AND (ISNULL(P.IsCompanyInternal, 0) = 0 AND @ExcludeInternalPracticesLocal  = 1 OR @ExcludeInternalPracticesLocal = 0)
			GROUP BY P.PracticeId,
					 p.Name,
					 CIB.Amount,
					 C.MonthStartDate
		)
		,
		
	 CTEMilestonePersonSchedule
		AS
		(
		  SELECT  m.[MilestoneId],
				  mp.PersonId,
				  m.ProjectId,
				  m.IsHourlyAmount,
				  mpe.MilestonePersonId,
				  dbo.PersonProjectedHoursPerDay(cal.DayOff,cal.companydayoff,cal.TimeoffHours,mpe.HoursPerDay) AS HoursPerDay,
				  cal.Date,
				  mpe.StartDate AS EntryStartDate,
				  mpe.Amount
		   FROM dbo.[Milestone] AS m
		   INNER JOIN dbo.MilestonePerson AS mp ON m.[MilestoneId] = mp.[MilestoneId]
		   INNER JOIN dbo.MilestonePersonEntry AS mpe ON mp.MilestonePersonId = mpe.MilestonePersonId
		  INNER JOIN dbo.PersonCalendarAuto AS cal ON (cal.Date BETWEEN mpe.Startdate AND mpe.EndDate AND cal.PersonId = mp.PersonId)
		),
		CTEFinancialsRetroSpective
		AS
		(
			SELECT  
				r.Date,
				p.PracticeId,
				CASE
					WHEN r.IsHourlyAmount = 1 OR s.HoursPerDay = 0
					THEN ISNULL(m.Amount*m.HoursPerDay, 0)
					ELSE ISNULL(r.MilestoneDailyAmount * m.HoursPerDay / s.HoursPerDay, r.MilestoneDailyAmount)
				END AS PersonMilestoneDailyAmount
			FROM  (			  
						SELECT -- Milestones with a fixed amount
							m.MilestoneId,
							m.ProjectId,
							p.ProjectStatusId,
							prac.PracticeId,
							prac.[IsCompanyInternal],
							cal.Date,
							m.IsHourlyAmount,
							ISNULL((m.Amount / (SELECT  SUM(HoursPerDay)
												FROM CTEMilestonePersonSchedule m1 WHERE  m1.MileStoneId = m.MilestoneId
										)) * ISNULL(d.HoursPerDay, 0),
									(CASE (DATEDIFF(dd, m.StartDate, m.ProjectedDeliveryDate) + 1)
										WHEN 0 THEN 0
										ELSE m.Amount / (DATEDIFF(dd, m.StartDate, m.ProjectedDeliveryDate) + 1)
									END)) AS MilestoneDailyAmount,
							p.Discount
						FROM dbo.Milestone AS m
						INNER JOIN dbo.Calendar AS cal ON cal.Date BETWEEN m.StartDate AND m.ProjectedDeliveryDate
															AND  cal.Date  BETWEEN @StartDateLocal AND	 @EndDateLocal
						INNER JOIN dbo.Project AS p ON m.ProjectId = p.ProjectId
						INNER JOIN dbo.Practice AS prac ON p.PracticeId = prac.PracticeId
						LEFT JOIN (
									SELECT  ps1.[MilestoneId],
									SUM(ps1.HoursPerDay )   HoursPerDay,
									ps1.Date
									FROM  CTEMilestonePersonSchedule ps1
								GROUP BY ps1.Date,ps1.MilestoneId
								) d ON d.date = cal.Date and m.MilestoneId = d.MileStoneId
					WHERE m.IsHourlyAmount = 0 
						 
					UNION ALL

					SELECT ps2.[MilestoneId],
							ps2.ProjectId,
							P.ProjectStatusId,
							prac.PracticeId,
							prac.[IsCompanyInternal],
							ps2.Date,
							ps2.IsHourlyAmount,
							ISNULL(SUM(ps2.Amount *( ps2.HoursPerDay )),0) MilestoneDailyAmount,
							MAX(p.Discount) AS Discount
					FROM CTEMilestonePersonSchedule ps2
						INNER JOIN dbo.Project AS p ON ps2.ProjectId = p.ProjectId
						INNER JOIN dbo.Practice AS prac ON p.PracticeId = prac.PracticeId
					WHERE ps2.IsHourlyAmount = 1 AND ps2.Date  BETWEEN @StartDateLocal AND	 @EndDateLocal
					GROUP BY ps2.MilestoneId, ps2.ProjectId, ps2.Date, ps2.IsHourlyAmount,
						P.ProjectStatusId,prac.PracticeId,prac.[IsCompanyInternal]
				) AS r
			-- Linking to persons
			LEFT JOIN  CTEMilestonePersonSchedule m ON m.MilestoneId = r.MilestoneId AND m.Date = r.Date AND m.PersonId IS NOT NULL
			LEFT JOIN (
						SELECT  ps3.[MilestoneId],
								SUM(ps3.HoursPerDay )   HoursPerDay,
								ps3.Date
						FROM  CTEMilestonePersonSchedule ps3
						GROUP BY ps3.Date,ps3.MilestoneId
						) AS s  ON s.Date = r.Date AND s.MilestoneId = r.MilestoneId 
			-- Salary
			LEFT JOIN  dbo.Pay AS p  ON p.Person  = m.PersonId AND p.StartDate <= r.Date AND p.EndDate > r.Date
			WHERE r.Date  BETWEEN @StartDateLocal AND	 @EndDateLocal
			AND (    ( @ShowProjectedLocal = 1 AND r.ProjectStatusId = 2 )
							OR ( @ShowActiveLocal = 1 AND r.ProjectStatusId = 3 )
							OR ( @ShowCompletedLocal = 1 AND r.ProjectStatusId = 4 )
							OR ( @showInternalLocal = 1 AND r.ProjectStatusId = 6 ) -- Internal
							OR ( @ShowExperimentalLocal = 1 AND r.ProjectStatusId = 5 )
							OR ( @ShowProposedLocal = 1 AND r.ProjectStatusId = 7 ) -- Proposed
							OR ( @ShowInactiveLocal = 1 AND r.ProjectStatusId = 1 ) -- Inactive
							OR ( @ShowAtRiskLocal = 1 AND r.ProjectStatusId = 8 )
						)
			GROUP BY r.Date,p.PracticeId,r.IsHourlyAmount,  m.HoursPerDay,s.HoursPerDay, m.Amount, r.MilestoneDailyAmount
			  ) 

	  SELECT PA.PracticeId,
			 PA.Name,
			 PA.MonthStartDate,
			 PA.Amount,
			 B.Revenue
	  FROM PracticeAreas PA
	  LEFT JOIN 
			(	SELECT f.PracticeId,
				   C.MonthStartDate AS MonthStartDate,
				   ISNULL(SUM(f.PersonMilestoneDailyAmount),0) AS Revenue
				FROM CTEFinancialsRetroSpective f
				INNER JOIN dbo.Calendar C ON C.Date = f.Date
				GROUP BY f.PracticeId, C.MonthStartDate
			) B
	  ON PA.PracticeId = B.PracticeId AND B.MonthStartDate = PA.MonthStartDate
	ORDER BY PA.Name 
	END
END

