﻿-- =============================================
-- Author:		Nikita Goncharenko
-- Create date: 2009-12-08
-- Description:	Inserts new time entry record
-- =============================================
CREATE PROCEDURE [dbo].[TimeEntryInsert]
    @TimeEntryId INT OUT ,
    @EntryDate DATETIME ,
    @MilestoneDate DATETIME ,
    @MilestonePersonId INT ,
    @ActualHours REAL ,
    @ForecastedHours REAL ,
    @TimeTypeId INT ,
    @ModifiedBy INT ,
    @Note VARCHAR(1000) ,
	@IsChargeable BIT,
    @DefaultMpId INT ,
    @IsCorrect BIT ,
    @PersonId INT -- id of the person that this milestone is about
				  -- it's needed when we're assiging person to the default
				  --	milestone, so it's not possible to extract id
				  --	from the milestonepersonid parameter
AS 
    BEGIN
        SET NOCOUNT ON ;
	
        BEGIN TRANSACTION
		
		DECLARE @CurrentPMTime DATETIME,
				@PTOTimeTypeId INT,
				@HolidayTimeTypeId INT,
				@Today DATETIME
		SET @CurrentPMTime = dbo.InsertingTime()
		
		SELECT @PTOTimeTypeId = TimeTypeId
		FROM TimeType
		WHERE Name = 'PTO'
		
		SELECT @HolidayTimeTypeId = TimeTypeId
		FROM TimeType
		WHERE Name = 'Holiday'

		IF @TimeTypeId = @PTOTimeTypeId
		BEGIN
			SET @Note = 'PTO'
		END

		SET @Today = CONVERT(DATETIME, CONVERT(DATE, dbo.GettingPMTime(GETUTCDATE())))
		
        --DECLARE @IsChargeable BIT
	
	-- If it's default milestone, create MPE for it
        IF @MilestonePersonId = 0 
            EXECUTE dbo.MilestonePersonEntryCreateProgrammatically @PersonId = @PersonId, --  int
                @MilestoneId = @DefaultMpId, @MilestoneDate = @PersonId, --  datetime
                @ActualHours = @ActualHours, --  real
                @DefaultMpId = @DefaultMpId,
                @NewMilestonePersonId = @MilestonePersonId OUTPUT
	
	--	Fill forecasted hours field if not present
        IF ( @ForecastedHours = 0 ) 
            BEGIN
                SELECT  @ForecastedHours = mpe.HoursPerDay
	--                  , @IsChargeable = m.IsChargeable
                FROM    dbo.MilestonePersonEntry AS mpe
                        INNER JOIN dbo.MilestonePerson AS mp ON mpe.MilestonePersonId = mp.MilestonePersonId
                        INNER JOIN dbo.Milestone AS m ON mp.MilestoneId = m.MilestoneId
                WHERE   mpe.MilestonePersonId = @MilestonePersonId
                        AND @MilestoneDate BETWEEN mpe.StartDate
                                           AND     ISNULL(mpe.EndDate,
                                                          dbo.GetFutureDate())
            END

		-- Check if the milestone date is inside corresponding milestone 
		--	start and end date interval
		IF NOT EXISTS (
						SELECT 1 
						FROM dbo.MilestonePersonEntry mpe
						WHERE mpe.MilestonePersonId = @MilestonePersonId
								AND @MilestoneDate BETWEEN mpe.StartDate AND ISNULL(mpe.EndDate, '2/3/2099'))
		BEGIN 
			DECLARE @MStart VARCHAR(20)
			DECLARE @MEnd VARCHAR(20)
		
			SELECT @MStart = CONVERT(VARCHAR, mpe.StartDate, 101), 
					 @MEnd = CONVERT(VARCHAR, mpe.EndDate, 101)
			FROM dbo.MilestonePersonEntry mpe
			WHERE mpe.MilestonePersonId = @MilestonePersonId	
		
			RAISERROR (N'The date you record time entry for should be between %s and %s.',
						16, 1, @MStart, @MEnd)
		END
		ELSE IF (@TimeTypeId = @PTOTimeTypeId)
				AND EXISTS (SELECT 1 FROM TimeEntries TE
									JOIN MilestonePerson MP ON MP.MilestonePersonId = TE.MilestonePersonId AND MP.PersonId = @PersonId
									WHERE TE.MilestoneDate = @MilestoneDate 
									AND TE.TimeTypeId = @PTOTimeTypeId)
		BEGIN
			RAISERROR(N'PTO Time Entry for the same day already exists', 16, 1)
		END
		ELSE 
		BEGIN
			INSERT  INTO [dbo].[TimeEntries]
					( [EntryDate] ,
					  [MilestoneDate] ,
					  [ModifiedDate] ,
					  [MilestonePersonId] ,
					  [ActualHours] ,
					  [ForecastedHours] ,
					  [TimeTypeId] ,
					  [ModifiedBy] ,
					  [Note] ,
					  [IsChargeable] ,
					  [IsCorrect],
					  [IsAutoGenerated]
					)
			VALUES  ( @CurrentPMTime ,
					  @MilestoneDate ,
					  @CurrentPMTime ,
					  @MilestonePersonId ,
					  @ActualHours ,
					  @ForecastedHours ,
					  @TimeTypeId ,
					  @ModifiedBy ,
					  @Note ,
					  @IsChargeable ,
					  @IsCorrect,
					  0
					)			
				
			SET @TimeEntryId = SCOPE_IDENTITY()	
							

			IF @TimeTypeId = @PTOTimeTypeId
				AND NOT EXISTS (SELECT 1 FROM TimeEntries TE
								JOIN MilestonePerson MP ON MP.MilestonePersonId = TE.MilestonePersonId AND MP.PersonId = @PersonId
								WHERE TE.MilestoneDate = @MilestoneDate 
									AND ((TE.TimeTypeId = @PTOTimeTypeId AND MP.MilestonePersonId <> @MilestonePersonId)
										OR TE.TimeEntryId = @HolidayTimeTypeId)
								)
				AND EXISTS (SELECT 1
							FROM dbo.Calendar AS cal
							WHERE cal.Date = @MilestoneDate AND cal.DayOff = 0)
			BEGIN
			
				IF NOT EXISTS (SELECT 1
								FROM dbo.PersonCalendar
								WHERE PersonId = @PersonId AND Date = @MilestoneDate)
				BEGIN
					INSERT INTO dbo.PersonCalendar (Date, PersonId, DayOff, ActualHours, TimeTypeId, IsFromTimeEntry)
					VALUES (@MilestoneDate, @PersonId, 1, @ActualHours, @PTOTimeTypeId, 1)
				END
				ELSE
				BEGIN
					/*
						company holiday, but person wants to work, later company holiday removed, then now if person enter PTO timeentry, we need to update Person DayOff=1.
					*/
					UPDATE PC
					SET PC.DayOff = 1,
						PC.ActualHours = @ActualHours,
						PC.TimeTypeId = @PTOTimeTypeId,
						PC.IsFromTimeEntry = 1
					FROM PersonCalendar PC
					WHERE PC.PersonId = @PersonId AND PC.Date = @MilestoneDate
				END


				--Update PersonCalendarAuto table dayoff with PersonCalendar dayOff.	
				UPDATE ca
					SET DayOff = pc.DayOff
					FROM dbo.PersonCalendarAuto AS ca
						INNER JOIN dbo.v_PersonCalendar AS pc ON ca.date = pc.Date AND ca.PersonId = pc.PersonId AND pc.Date = @MilestoneDate
					WHERE ca.PersonId = @PersonId
			END
		
		END

		COMMIT 
    END




