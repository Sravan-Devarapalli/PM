CREATE PROCEDURE [dbo].[UpdateFixedFeeMilestoneDiscount]
(
	@MilestoneId              INT,
	@UpdateMonthlyRevenues    BIT = 0
)
AS
BEGIN
	   SET NOCOUNT ON;
		SET ANSI_WARNINGS OFF

		DECLARE @IsHourlyMilestone BIT=0,
				@IsDiscountAtMilestone INT=0,
				@MilestoneIdLocal INT,
				@DiscountType INT,
				@MilestoneDiscount DECIMAL(18,2),
				@MilestoneFixedAmount DECIMAL(18,2),
				@IsAmountAtMilestone BIT=0,
				@TotalPersonRevenue DECIMAL(18,2)

		SELECT @MilestoneIdLocal=@MilestoneId

		SELECT @IsHourlyMilestone = M.IsHourlyAmount, 
			   @IsDiscountAtMilestone=M.IsDiscountAtMilestone, 
			   @DiscountType=M.DiscountType, 
			   @IsAmountAtMilestone = M.IsAmountAtMilestone,
			   @MilestoneDiscount=M.Discount, 
			   @MilestoneFixedAmount=M.Amount
		FROM Milestone M WHERE M.MilestoneId=@MilestoneIdLocal

		IF @IsHourlyMilestone = 0
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
						WHERE m.MilestoneId = @MilestoneIdLocal
						GROUP BY  m.ProjectId,m.[MilestoneId],mp.PersonId,cal.Date,m.IsHourlyAmount ,m.IsDefault,MPE.Id,MPE.Amount

						CREATE CLUSTERED INDEX cix_MileStoneEntries1 ON #MileStoneEntries1( ProjectId,[MilestoneId],PersonId,[Date],Id,Amount)

					SELECT	ME.ProjectId,
							ME.MilestoneId,
							ME.PersonId As PersonId,
							c.Date,
							ISNULL(ME.Amount*ME.HoursPerDay, 0) as MilestonePersonAmount,
							ME.Id as EntryId
					INTO #cteFinancialsRetrospective
					FROM  #MileStoneEntries1 AS ME 
					INNER JOIN dbo.Calendar C ON c.Date = ME.Date
					GROUP BY ME.ProjectId,ME.MilestoneId, ME.PersonId,c.Date,  ME.Amount, ME.Id, ME.HoursPerDay
	
						CREATE CLUSTERED INDEX CIX_cteFinancialsRetrospectiveActualHours ON #cteFinancialsRetrospective(ProjectId, MilestoneId,
							PersonId,
							Date
							)
	

						SELECT  f.EntryId,
							    f.MilestoneId,
							    f.MilestonePersonAmount
						INTO #FinancialsRetro
						FROM #cteFinancialsRetrospective f
						WHERE f.MilestoneId=@MilestoneIdLocal

						--;WITH FinancialsRetro AS 
						--(
						--SELECT f.EntryId,
						--	   f.MilestoneId,
						--	   f.MilestonePersonAmount
						--FROM v_FinancialsRetrospective f
						--WHERE f.MilestoneId = @MilestoneIdLocal
						--)
					
						SELECT f.EntryId,
							   f.MilestoneId,
							   SUM(f.MilestonePersonAmount) AS MilestonePersonAmount
						INTO #MilestonePersonFinancials
						FROM #FinancialsRetro AS f
						WHERE f.MilestoneId = @MilestoneIdLocal
						GROUP BY f.EntryId,f.MilestoneId

						SELECT m.EntryId,
							   m.MilestonePersonAmount/mpe.Amount as TotalHours
						INTO #MilestonePersonTotalHours 
						FROM #MilestonePersonFinancials m
						JOIN MilestonePersonEntry mpe on mpe.Id=m.EntryId

						SELECT @TotalPersonRevenue = SUM(MPA.MilestonePersonAmount)
						FROM #MilestonePersonFinancials MPA

						DECLARE @LockedResourceDiscount DECIMAL(18,2),
								@LockedResourceRevenue  DECIMAL(18,2),
								@NetResourceRevenue DECIMAL(18,2)

						SELECT  @LockedResourceDiscount=ISNULL(SUM(ISNULL(MPE.Discount*MPH.TotalHours,0)),0)
						FROM MilestonePersonEntry MPE
						JOIN #MilestonePersonTotalHours MPH on MPH.EntryId=MPE.Id
						WHERE  MPE.LockDiscount=1

						SELECT @LockedResourceRevenue = ISNULL(SUM(ISNULL(MF.MilestonePersonAmount,0)),0)
						FROM MilestonePersonEntry MPE
						JOIN #MilestonePersonFinancials MF on MF.EntryId=MPE.Id 
						WHERE MPE.LockDiscount=1
		      
						SELECT @NetResourceRevenue = @TotalPersonRevenue - @LockedResourceRevenue

						if(@MilestoneFixedAmount IS NULL)
						BEGIN
							SELECT @MilestoneFixedAmount = 0
							DELETE from FixedMilestoneMonthlyRevenue WHERE MilestoneId= @MilestoneIdLocal
						END 
						if(@MilestoneDiscount IS NULL)
						BEGIN
							SELECT @MilestoneDiscount =0
						END 
						
			IF (@IsDiscountAtMilestone=1 OR @IsDiscountAtMilestone=0)
			BEGIN
				IF(@DiscountType=2) --percentage
				BEGIN
					
					DECLARE @MilestoneDiscountAmountLocal DECIMAL(18,2), @LockedResourceDiscountLocal DECIMAL(18,2), @NetDiscount DECIMAL(18,2)

					IF(@IsAmountAtMilestone = 1)
					BEGIN
						SELECT @MilestoneDiscount = @MilestoneFixedAmount - @TotalPersonRevenue

						UPDATE M
						SET M.Discount = @MilestoneDiscount*100/@TotalPersonRevenue
						FROM Milestone M
						WHERE M.MilestoneId = @MilestoneIdLocal
					END
					
					SELECT @MilestoneDiscountAmountLocal = ISNULL(m.Discount*@TotalPersonRevenue/100,0)
					FROM Milestone M
					WHERE M.MilestoneId = @MilestoneIdLocal

					IF(@IsAmountAtMilestone = 0)
					BEGIN

						UPDATE M
						SET M.Amount =@MilestoneDiscountAmountLocal+@TotalPersonRevenue
						FROM Milestone M
						WHERE M.MilestoneId = @MilestoneIdLocal
					END
					 
					SELECT @LockedResourceDiscountLocal = ISNULL(SUM(ISNULL(MPE.Discount*MF.MilestonePersonAmount/100,0)),0)
					FROM MilestonePersonEntry MPE
					JOIN #MilestonePersonFinancials MF on MF.EntryId=MPE.Id
					WHERE MPE.LockDiscount = 1

					SELECT @NetDiscount = @MilestoneDiscountAmountLocal- @LockedResourceDiscountLocal

					UPDATE MPE
					SET MPE.Discount=@NetDiscount*100/@NetResourceRevenue
					FROM MilestonePersonEntry MPE
					JOIN MilestonePerson MP on MP.MilestonePersonId=MPE.MilestonePersonId
					JOIN Milestone M on M.MilestoneId= MP.MilestoneId
					WHere m.MilestoneId=@MilestoneIdLocal and MPE.LockDiscount = 0

				END
				ELSE
				BEGIN
				IF(@DiscountType=1) --dollar
				BEGIN
					
					DECLARE @RemDiscount DECIMAL(18,2)

					IF(@IsAmountAtMilestone = 1)
					BEGIN
						SELECT @MilestoneDiscount = @MilestoneFixedAmount - @TotalPersonRevenue

						UPDATE M
						SET M.Discount = @MilestoneDiscount
						FROM Milestone M
						WHERE MilestoneId =@MilestoneIdLocal
					END
					ELSE
					BEGIN
						UPDATE M
						SET M.Amount = @MilestoneDiscount+@TotalPersonRevenue
						FROM Milestone M
						WHERE MilestoneId =@MilestoneIdLocal
					END
					
						SELECT @RemDiscount = @MilestoneDiscount - @LockedResourceDiscount
					
						UPDATE MPE
						SET MPE.Discount=(MF.MilestonePersonAmount*@RemDiscount/@NetResourceRevenue)/MPH.TotalHours
						FROM MilestonePersonEntry MPE
						JOIN #MilestonePersonFinancials MF on MF.EntryId=MPE.Id
						JOIN #MilestonePersonTotalHours MPH on MPH.EntryId =MPE.Id
						WHERE MPE.LockDiscount=0
				END
				END
			END
			ELSE 
			IF(@IsDiscountAtMilestone=2) --at resource level
			BEGIN
				DECLARE @DiscountLocal DECIMAL(18,2), @NetResourceDiscount DECIMAL(18,2)

				IF(@DiscountType=2)--percentage
				BEGIN
					SELECT @MilestoneDiscount=ISNULL(SUM(ISNULL(MPE.Discount*MF.MilestonePersonAmount/100, 0)),0)
					FROM MilestonePersonEntry MPE
					JOIN #MilestonePersonFinancials MF on MF.EntryId=MPE.Id

					IF(@IsAmountAtMilestone = 1)
					BEGIN
						SELECT @DiscountLocal = @MilestoneFixedAmount - @TotalPersonRevenue

						SELECT @LockedResourceDiscount= ISNULL(SUM(ISNULL(MPE.Discount*MF.MilestonePersonAmount/100,0)),0)
						FROM MilestonePersonEntry MPE
						JOIN #MilestonePersonFinancials MF on MF.EntryId=MPE.Id
						WHERE MPE.LockDiscount = 1

						SELECT @NetResourceDiscount= @DiscountLocal - @LockedResourceDiscount

						UPDATE MPE
						SET MPE.Discount = @NetResourceDiscount*100/@NetResourceRevenue
						FROM MilestonePersonEntry MPE
						JOIN #MilestonePersonFinancials MF on MF.EntryId=MPE.Id
						WHERE MPE.LockDiscount=0

						UPDATE M
						SET M.Discount = CASE WHEN @TotalPersonRevenue !=0 THEN @DiscountLocal*100/@TotalPersonRevenue ELSE 0 END
						FROM Milestone M
						WHERE M.MilestoneId = @MilestoneIdLocal
					END
					ELSE
					BEGIN
						DECLARE @discountFromResourceLocal DECIMAL(18,2), @lockedDiscountFromRescourceLocal DECIMAL(18,2)

						SELECT @discountFromResourceLocal=SUM(ISNULL(MPE.Discount*MF.MilestonePersonAmount/100,0))
						FROM MilestonePersonEntry MPE
						JOIN #MilestonePersonFinancials MF on MF.EntryId=MPE.Id

						SELECT @lockedDiscountFromRescourceLocal = SUM(ISNULL(MPE.Discount*MF.MilestonePersonAmount/100,0))
						FROM MilestonePersonEntry MPE
						JOIN #MilestonePersonFinancials MF on MF.EntryId=MPE.Id
						WHERE MPE.LockDiscount = 1

						UPDATE M  
						SET M.Discount=@discountFromResourceLocal*100/@TotalPersonRevenue,
							M.Amount=@TotalPersonRevenue+@discountFromResourceLocal
						FROM Milestone M 
						WHERE M.MilestoneId=@MilestoneIdLocal 

						SELECT @NetResourceDiscount = @discountFromResourceLocal - @lockedDiscountFromRescourceLocal

						--UPDATE MPE
						--SET MPE.Discount = @NetResourceDiscount*100/@NetResourceRevenue
						--FROM MilestonePersonEntry MPE
						--JOIN #MilestonePersonFinancials MF on MF.EntryId=MPE.Id
						--WHERE MPE.LockDiscount=0

					END
					
				END
				ELSE 
				IF(@DiscountType=1)--dollar
				BEGIN
					
					if(@IsAmountAtMilestone = 1)
					BEGIN
						
						SELECT @DiscountLocal = @MilestoneFixedAmount - @TotalPersonRevenue

						SELECT @NetResourceDiscount= @DiscountLocal - @LockedResourceDiscount

						UPDATE MPE
						SET MPE.Discount=(MF.MilestonePersonAmount*@NetResourceDiscount/@NetResourceRevenue)/MPH.TotalHours
						FROM MilestonePersonEntry MPE
						JOIN #MilestonePersonFinancials MF on MF.EntryId=MPE.Id
						JOIN #MilestonePersonTotalHours MPH on MPH.EntryId=MPE.Id
						WHERE MPE.LockDiscount=0

						UPDATE M
						SET M.Discount = @DiscountLocal
						FROM Milestone M
						WHERE M.MilestoneId = @MilestoneIdLocal

					END
					ELSE
					BEGIN

						DECLARE @discountFromResource DECIMAL(18,2)
						SELECT @discountFromResource=SUM(ISNULL(MPE.Discount*MPH.TotalHours,0))
						FROM MilestonePersonEntry MPE
						JOIN #MilestonePersonTotalHours MPH on MPH.EntryId=MPE.Id

						UPDATE M  
						SET M.Discount=@discountFromResource,
							M.Amount=@TotalPersonRevenue+@discountFromResource
						FROM Milestone M 
						WHERE M.MilestoneId=@MilestoneIdLocal 

						--UPDATE MPE
						--SET MPE.Discount=(MF.MilestonePersonAmount*(@discountFromResource - @LockedResourceDiscount)/@NetResourceRevenue)/MPH.TotalHours
						--FROM MilestonePersonEntry MPE
						--JOIN #MilestonePersonFinancials MF on MF.EntryId=MPE.Id
						--JOIN #MilestonePersonTotalHours MPH on MPH.EntryId =MPE.Id
						--WHERE MPE.LockDiscount=0
					END
				END
			END

			IF(@IsAmountAtMilestone = 0 OR @UpdateMonthlyRevenues = 1)
			BEGIN

				DECLARE @start_date DATETIME, @end_date DATETIME, @numberOfMonths INT,  @monthlyAMount DECIMAL(18, 2), @milestoneAmount DECIMAL(18, 2)
				SELECT @start_date = StartDate, @end_date= ProjectedDeliveryDate, @milestoneAmount = Amount FROM Milestone Where MilestoneId= @MilestoneIdLocal

				SELECT @numberOfMonths = DATEDIFF(month, @start_date, @end_date)+1
				IF(@numberOfMonths > 1)
				BEGIN
					SELECT @monthlyAMount = @milestoneAmount/@numberOfMonths

					DELETE from FixedMilestoneMonthlyRevenue WHERE MilestoneId= @MilestoneIdLocal

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
					SELECT @MilestoneIdLocal,
						   C.cte_start_date,
						   C.cte_end_date,
						   @monthlyAMount
					FROM CTE C 

				END
		    END

			DROP TABLE #MilestonePersonFinancials
			DROP TABLE  #MilestonePersonTotalHours
		END

END

