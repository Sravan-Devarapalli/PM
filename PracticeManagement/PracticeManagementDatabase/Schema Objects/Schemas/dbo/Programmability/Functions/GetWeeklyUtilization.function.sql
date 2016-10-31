﻿-- =============================================
-- Author:		Nikita Goncharenko
-- Create date: 2009-11-20
-- Description:	Get utilization report by weeks for given person
-- =============================================
CREATE FUNCTION [dbo].[GetWeeklyUtilization]
(
	@PersonId INT,
	@StartDate DATETIME,
	@Step INT = 7,
	@DaysForward INT = 184,
	@ActiveProjects BIT = 1,
	@ProjectedProjects BIT = 1,
	@ExperimentalProjects BIT = 1,
	@ProposedProjects BIT =1,
	@InternalProjects	BIT = 1,
	@CompletedProjects BIT = 1 ,
	@AtRiskProjects BIT =1
)
RETURNS VARCHAR(2500)
AS
BEGIN
	DECLARE @rep VARCHAR(2500)
	SET @rep = ''

    -- Iterator through days
    DECLARE @w INT
    SET @w = 0
    
    DECLARE @start DATETIME
    DECLARE @end DATETIME
    DECLARE @wUtil INT
    DECLARE @av INT 
    DECLARE @vac INT = 0
    DECLARE @timeScale INT 
	DECLARE @Delta INT
	DECLARE @EndRange DATETIME
	
	SET @EndRange = DATEADD(dd , @DaysForward, @StartDate) - 1
	IF(@Step = 7)
	BEGIN
		IF(DATEPART(DW,@StartDate)>2)
		BEGIN
			SELECT @StartDate = @StartDate - DATEPART(DW,@StartDate)+2
		END
		IF(DATEPART(DW,@StartDate)<7)
		BEGIN
			SELECT @EndRange = DATEADD(dd , 7-DATEPART(DW,@EndRange), @EndRange)
		END
	END
	ELSE IF (@Step = 30)
	BEGIN
                
		IF(DATEPART(DW,@StartDate)>2)
		BEGIN
			SELECT @StartDate = @StartDate - DATEPART(DW,@StartDate)+2
		END
		IF(DATEPART(DW,@EndRange)<7)
		BEGIN
			SELECT @EndRange = DATEADD(dd , 7-DATEPART(DW,@EndRange), @EndRange)
		END
	END
    SELECT @DaysForward = DATEDIFF(DD,@StartDate,@EndRange)+1
    -- Iterate through all days
    WHILE (@w*@Step < @DaysForward)
    BEGIN

		IF(@Step = 30)
		BEGIN
		
			IF(@EndRange >= DATEADD(mm , (@w+1), @StartDate)-1)
			BEGIN
			
				SET @start = DATEADD(mm , @w, @StartDate)
				SET @end = DATEADD(mm , (@w+1), @StartDate)-1
			END
			ELSE
			BEGIN
				SET @start = DATEADD(mm , @w, @StartDate)
				SET @end = @EndRange
			END
		END
		ELSE
		BEGIN
			SET @start = DATEADD(day, @w*@Step, @StartDate)		

			SET @Delta = @DaysForward - (@w*@Step - 1)
			IF(@Delta <= @Step)
			BEGIN
				SET @end = DATEADD(day, @DaysForward - 1, @StartDate)
				SET @w = @w + 1
			END
			ELSE
				SET @end = DATEADD(day, (@w + 1)*@Step - 1, @StartDate)
		END
		SELECT @timeScale = Timescale FROM dbo.GetLatestPayWithInTheGivenRange(@start,@end) WHERE PersonId = @PersonId
		
		IF @Step = 1 AND DATEPART(dw, @start)IN(7,1)
		SET @wUtil = 0
		ELSE
		BEGIN
			SET @vac = dbo.GetPersonVacationDays(@PersonId, @start, 1)
			IF @vac >= @Step 
				SET @wUtil = -2
			ELSE
			BEGIN
			IF @timeScale = 2
					SET @vac = dbo.GetNumberHolidayDaysWithWeekends(@PersonId, @start, @end)
				IF @vac >= @Step
					SET @wUtil = -1
				ELSE  
				BEGIN
					SET @av = dbo.GetNumberAvaliableHours(@PersonId, @start, @end, @ActiveProjects, @ProjectedProjects, @ExperimentalProjects ,@ProposedProjects,@InternalProjects,@CompletedProjects,@AtRiskProjects)
			
					IF (@av = 0 OR @av IS NULL)
						SET @wUtil = 0
					ELSE 		
						SET @wUtil = CEILING(
							100*ISNULL(dbo.GetNumberProjectedHours(@PersonId, @start, @end, @ActiveProjects, @ProjectedProjects, @ExperimentalProjects,@ProposedProjects ,@InternalProjects,@CompletedProjects,@AtRiskProjects), 0) / 
								@av)
				END
			END 
		END	
		SET @rep = @rep + CONVERT(VARCHAR, ISNULL(@wUtil, 0)) + ','
		
		SET @w = @w + 1
    END

	RETURN @rep

END

