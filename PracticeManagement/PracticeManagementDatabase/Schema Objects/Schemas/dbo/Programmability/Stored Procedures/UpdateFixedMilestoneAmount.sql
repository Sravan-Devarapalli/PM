CREATE PROCEDURE [dbo].[UpdateFixedMilestoneAmount]
(
	@MilestoneId              INT
)
AS
BEGIN
	    SET NOCOUNT ON;
		SET ANSI_WARNINGS OFF

		DECLARE @IsHourlyMilestone BIT=0,
				@MilestoneAmount DECIMAL(18,2),
				@IsAmountAtMilestone BIT 

		SELECT @IsHourlyMilestone = M.IsHourlyAmount,  @MilestoneAmount=M.Amount, @IsAmountAtMilestone =M.IsAmountAtMilestone
		FROM Milestone M WHERE M.MilestoneId=@MilestoneId

	   IF( @IsHourlyMilestone = 0 and @IsAmountAtMilestone = 0)
	   BEGIN

		SELECT  m.ProjectId,  
				m.[MilestoneId],  
				mp.PersonId As PersonId,  
				cal.Date,  
				MPE.Id,  
				MPE.Amount,  
				SUM(CONVERT(DECIMAL(4,2),CASE WHEN cal.DayOff = 0  THEN mpe.HoursPerDay -- No Time-off and no company holiday  
				 WHEN cal.companydayoff = 1 OR ISNULL(cal.TimeoffHours,8) = 8 THEN 0 -- only company holiday OR person complete dayoff  
				 ELSE mpe.HoursPerDay * (1-(cal.TimeoffHours/8)) --person partial day off  
				END)) AS HoursPerDay  
			  INTO #MileStoneEntries1  
				FROM dbo.Project P  
				INNER JOIN dbo.[Milestone] AS m ON P.ProjectId=m.ProjectId AND p.IsAllowedToShow = 1 AND p.projectid != 174  
				INNER JOIN dbo.MilestonePerson AS mp ON m.[MilestoneId] = mp.[MilestoneId]  
				INNER JOIN dbo.MilestonePersonEntry AS mpe ON mp.MilestonePersonId = mpe.MilestonePersonId  
				INNER JOIN dbo.PersonCalendarAuto AS cal ON cal.Date BETWEEN mpe.Startdate AND mpe.EndDate AND cal.PersonId = mp.PersonId  
			  WHERE m.MilestoneId = @MilestoneId  
			  GROUP BY  m.ProjectId,m.[MilestoneId],mp.PersonId,cal.Date,MPE.Id,MPE.Amount  
  
			  CREATE CLUSTERED INDEX cix_MileStoneEntries1 ON #MileStoneEntries1( ProjectId,[MilestoneId],PersonId,[Date],Id,Amount)  
  
			 SELECT ME.MilestoneId,  
				   SUM(ISNULL(ME.Amount*ME.HoursPerDay, 0)) as MilestonePersonAmount
			 INTO #cteFinancialsRetrospective  
			 FROM  #MileStoneEntries1 AS ME   
			 GROUP BY ME.ProjectId,ME.MilestoneId 
			

			UPDATE M
			SET M.Amount = cte.MilestonePersonAmount
			FROM Milestone M
			JOIN #cteFinancialsRetrospective cte on m.MilestoneId= cte.MilestoneId
			WHERE M.MilestoneId=@MilestoneId AND M.IsHourlyAmount=0
		
			DROP TABLE  #cteFinancialsRetrospective  
			drop table #MileStoneEntries1
		
		

			DECLARE @start_date DATETIME, @end_date DATETIME, @numberOfMonths INT,  @monthlyAMount DECIMAL(18, 2)
			SELECT @start_date = StartDate, @end_date= ProjectedDeliveryDate, @milestoneAmount = Amount FROM Milestone Where MilestoneId= @MilestoneId

			SELECT @numberOfMonths = DATEDIFF(month, @start_date, @end_date)+1
			IF(@numberOfMonths > 1)
			BEGIN
				SELECT @monthlyAMount = @milestoneAmount/@numberOfMonths

				DELETE from FixedMilestoneMonthlyRevenue WHERE MilestoneId= @MilestoneId

				;WITH CTE AS
				(
					SELECT @start_date AS cte_start_date,CONVERT(DATETIME, EOMONTH(@start_date)) as cte_end_date, @monthlyAMount as amount
					UNION ALL
					SELECT DATEADD(MONTH, DATEDIFF(MONTH, 0, DATEADD(MONTH, 1, cte_start_date)), 0), 
					CASE WHEN (CONVERT(DATETIME, EOMONTH(DATEADD(MONTH, 1, cte_end_date))) > @end_date) THEN @end_date ELSE CONVERT(DATETIME, EOMONTH(DATEADD(MONTH, 1, cte_end_date))) END,
					@monthlyAMount
					FROM CTE
					WHERE EOMONTH(DATEADD(month, DATEDIFF(month, 0, cte_start_date), 0)) < @end_date   
				)
			
				INSERT INTO FixedMilestoneMonthlyRevenue (MilestoneId, StartDate, EndDate, Amount)
				SELECT @MilestoneId,
					   C.cte_start_date,
					   C.cte_end_date,
					   @monthlyAMount
				FROM CTE C 
			END

			END
END

