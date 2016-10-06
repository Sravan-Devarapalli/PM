﻿-- =============================================
-- Author:		Srinivas.M
-- Create date: 02-23-2012
-- Updated by:	Sainath C
-- Update date:	06-01-2012
-- Description: Insert/Update/Delete Time Off(s) for a person.
-- =============================================
CREATE PROCEDURE [dbo].[SaveTimeOff]
    (
      @StartDate DATETIME ,
      @EndDate DATETIME ,
      @DayOff BIT ,
      @PersonId INT ,
      @UserLogin NVARCHAR(255) ,
      @ActualHours REAL ,
      @TimeTypeId INT ,
      @ApprovedBy INT,
	  @OldStartDate DATETIME = NULL,
	  @IsFromAddTimeOffButton BIT = 0
    )
AS 
    BEGIN
	/*
		If DayOff = 1 
		{
			Insert entry into Personcalendar table
			
		}
		else
		{
			delete entry from PersonCalendar table.
	
			delete timeEntries of @TimeTypeId.
		}
		
		Delete TimeEtnries if there is no entry with DayOff = 1 in PersonCalendar and exists in TimeEntry table.
		Update TimeEntries if there is an entry with DayOff = 1 in PersonCalendar and exists with different ACTUAL HOURS in TimeEntry table.
		Insert TimeEntries if there is an entry with DayOff = 1 in PersonCalendar and not exists in TimeEntry table ONLY for w2salaried/w2hourly persons.
	*/
			EXEC dbo.SessionLogPrepare @UserLogin = @UserLogin
        DECLARE @Today			DATETIME ,
            @CurrentPMTime		DATETIME ,
            @ModifiedBy			INT ,
            @HolidayTimeTypeId	INT ,
            @Description		NVARCHAR(500) ,
            @ORTTimeTypeId		INT,
			@UnpaidTimeTypeId	INT,
			@W2SalaryId			INT,
			@W2HourlyId			INT,
			@IsUpdate			BIT=0,
			@SeriesId			INT = NULL,
			@SeriesStartDate   DATETIME,
			@SeriesEndDate     DATETIME

		   
        SELECT  @Today = dbo.GettingPMTime(GETUTCDATE()) ,
                @CurrentPMTime = dbo.InsertingTime() ,
                @HolidayTimeTypeId = dbo.GetHolidayTimeTypeId() ,
                @ORTTimeTypeId = dbo.GetORTTimeTypeId(),
				@UnpaidTimeTypeId = dbo.GetUnpaidTimeTypeId()
		SELECT	@W2SalaryId = TimescaleId FROM Timescale WHERE Name = 'W2-Salary'
		SELECT	@W2HourlyId = TimescaleId FROM Timescale WHERE Name = 'W2-Hourly'

        SELECT  @ModifiedBy = PersonId
        FROM    Person
        WHERE   Alias = @UserLogin
        SELECT  @Description = Name + '.'
        FROM    TimeType
        WHERE   TimeTypeId = @TimeTypeId
       
	--SELECT @ApprovedBy = CASE WHEN @ApprovedBy IS NOT NULL THEN @ApprovedBy ELSE @ModifiedBy END
			DECLARE @PersonCalendarLog TABLE(
											Id					INT,
											StartDate			DATETIME,
											EndDate			DATETIME,
											[PersonId]			INT,
											[DayOff]			BIT,
											[ActualHours]		REAL,
											[IsSeries]			BIT,
											[TimeTypeId]		INT,
											[SubstituteDate]	DATETIME ,
											[Description]		NVARCHAR(500) ,
											[IsFromTimeEntry]	BIT ,
											[ApprovedBy]		INT ,
											[SeriesId]          BIGINT,
											IsUpdate			INT, --P.IsUpdate = 0 FOR INSERT ,1 FOR UPDATE,2 FOR DELETE
											IsNewRow			BIT   --IsNewRow = 1 for insert into table,0 for others
										   )
        BEGIN TRY
            BEGIN TRANSACTION tran_SaveTimeOff

            IF @DayOff = 1 
                BEGIN
		/*
			. Adding/Updating the series of Offs then we need to exclude days after this series endDate and before this series startDate from the series.
			. If already other Offs present then we need to update them with new Off.
				
			Note:- If any changes(actualhours, worktype) done in PersonCalendar table then update those in TimeEntry table also.
		*/

		--If selected days has not atleast 1 working day, the we need to throw validation.
                    IF 1 > ( SELECT COUNT(*)
                             FROM   dbo.Calendar C
                                    LEFT JOIN dbo.PersonCalendar PC ON PC.PersonId = @PersonId
                                                              AND C.Date = PC.Date
                                    LEFT JOIN dbo.PersonCalendar Holiday ON PC.Date = Holiday.SubstituteDate
                                                              AND PC.PersonId = Holiday.PersonId
                             WHERE  C.Date BETWEEN @StartDate AND @EndDate
                                    AND C.DayOff = 0
                                    AND ( PC.Date IS NULL
                                          OR ( PC.Date IS NOT NULL
                                               AND PC.DayOff = 1
                                               AND Holiday.SubstituteDate IS NULL
                                             )
                                        )
                           ) 
                        BEGIN
                            RAISERROR('Selected day(s) are not working day(s). Please select any working day(s).', 16, 1)
                        END

		     --DELETE OLD Time-OFFS (AS PER #3133 DEFECT) 
			                SET @SeriesId = (SELECT TOP 1 SeriesId 
											FROM PersonCalendar PC
											WHERE PC.Date=@OldStartDate AND PC.PersonId=@PersonId AND PC.DayOff=1)
					SELECT @SeriesStartDate = MIN(P.Date),
							@SeriesEndDate = MAX(P.Date)
					FROM dbo.PersonCalendar P
					 WHERE P.SeriesId = @SeriesId
						 GROUP BY P.SeriesId

					IF @SeriesId IS NOT NULL
					BEGIN
					     SET @IsUpdate = 1
						 INSERT INTO @PersonCalendarLog(Id,StartDate,EndDate,SeriesId)
						 SELECT		ROW_NUMBER() OVER(ORDER BY MIN(P.Date)),
									MIN(P.Date),
									MAX(P.Date),
									@SeriesId
						 FROM dbo.PersonCalendar P
						 WHERE P.SeriesId = @SeriesId
						 GROUP BY P.SeriesId
						 
						UPDATE  P
						SET P.DayOff = PC.DayOff,
							P.PersonId = PC.PersonId,
							P.ActualHours = PC.ActualHours,
							P.IsSeries = PC.IsSeries,
							P.TimeTypeId = PC.TimeTypeId,
							P.SubstituteDate = PC.SubstituteDate,
							P.Description = PC.Description,
							P.IsFromTimeEntry =PC.IsFromTimeEntry,
							P.ApprovedBy = @ApprovedBy,
							P.IsUpdate = @IsUpdate,
							P.IsNewRow = 0
						FROM @PersonCalendarLog P
						JOIN PersonCalendar PC ON PC.SeriesId = P.SeriesId
					END

					DELETE  P
					FROM dbo.PersonCalendar P
                    WHERE P.SeriesId=(
									 SELECT SeriesId 
									 FROM PersonCalendar PC
									 WHERE PC.Date=@OldStartDate AND PC.PersonId=@PersonId AND PC.DayOff=1)
                    DECLARE @DaysExceptHolidays TABLE ( [Date] DATETIME )

                    INSERT  INTO @DaysExceptHolidays
                            SELECT  C.Date
                            FROM    dbo.Calendar C
                                    LEFT JOIN dbo.PersonCalendar PC ON PC.PersonId = @PersonId
                                                              AND C.Date = PC.Date
                            WHERE   C.Date BETWEEN @StartDate AND @EndDate
                                    AND ( C.DayOff = 0
                                          AND ( PC.Date IS NULL
                                                OR ( PC.DATE IS NOT NULL
                                                     AND PC.DayOff = 1
                                                     AND PC.TimeTypeId <> @HolidayTimeTypeId
                                                   )
                                              )
                                        )

				--delete old Offs.
				IF (@IsFromAddTimeOffButton = 0)
				BEGIN
				    SET @SeriesId = NULL
					SELECT TOP 1 @SeriesId = PC.SeriesId 
					FROM    dbo.PersonCalendar PC
					JOIN @DaysExceptHolidays DEH ON PC.PersonId = @PersonId
                    AND PC.Date = DEH.Date

					IF @SeriesId IS NOT NULL
					BEGIN
						SET @IsUpdate = 1
					END

					INSERT INTO @PersonCalendarLog(Id,StartDate,EndDate,SeriesId,IsUpdate)
					SELECT      ROW_NUMBER() OVER(ORDER BY CASE WHEN MIN(P.Date) > @StartDate THEN MIN(P.Date) ELSE @StartDate END),
								CASE WHEN MIN(P.Date) > @StartDate THEN MIN(P.Date) ELSE @StartDate END,
								CASE WHEN MAX(P.Date) > @EndDate THEN @EndDate ELSE MAX(P.Date) END,
								P.SeriesId,
								@IsUpdate
					FROM dbo.PersonCalendar P
					WHERE P.SeriesId = @SeriesId 
					GROUP BY P.SeriesId

					UPDATE  P
						SET P.DayOff = PC.DayOff,
							P.PersonId = PC.PersonId,
							P.ActualHours = PC.ActualHours,
							P.IsSeries = PC.IsSeries,
							P.TimeTypeId = PC.TimeTypeId,
							P.SubstituteDate = PC.SubstituteDate,
							P.Description = PC.Description,
							P.IsFromTimeEntry =0,
							P.ApprovedBy = @ApprovedBy,
							P.IsNewRow = 0
						FROM @PersonCalendarLog P
						JOIN PersonCalendar PC ON PC.SeriesId = P.SeriesId
				END
                    DELETE PC
                    FROM    dbo.PersonCalendar PC
                            JOIN @DaysExceptHolidays DEH ON PC.PersonId = @PersonId
                                                            AND PC.Date = DEH.Date

		--Insert all Offs.
                    INSERT  INTO PersonCalendar
                            ( PersonId ,
                              Date ,
                              DayOff ,
                              TimeTypeId ,
                              ActualHours ,
                              Description ,
                              
                              IsFromTimeEntry ,
                              ApprovedBy
                            )
                            SELECT  @PersonId ,
                                    DEH.Date ,
                                    @DayOff ,
                                    @TimeTypeId ,
                                    @ActualHours ,
                                    @Description ,
                                    0 ,
                                    @ApprovedBy
                            FROM    @DaysExceptHolidays DEH
                                    LEFT JOIN PersonCalendar PC ON PC.Date = DEH.Date
                                                              AND PC.PersonId = @PersonId
 

                 INSERT INTO @PersonCalendarLog(Id,StartDate,EndDate,SeriesId)
						 SELECT		
									ROW_NUMBER() OVER(ORDER BY MIN(DEH.Date)),
									MIN(DEH.Date),
									MAX(DEH.Date),
									PC.SeriesId
						 FROM @DaysExceptHolidays DEH
                              LEFT JOIN PersonCalendar PC ON PC.Date = DEH.Date
                                                              AND PC.PersonId = @PersonId
						 GROUP BY PC.SeriesId 

						UPDATE  P
						SET P.DayOff = @DayOff,
							P.PersonId = @PersonId,
							P.ActualHours =@ActualHours,
							P.IsSeries = PC.IsSeries,
							P.TimeTypeId = @TimeTypeId,
							P.SubstituteDate = PC.SubstituteDate,
							P.Description = @Description,
							P.IsFromTimeEntry =0,
							P.ApprovedBy = @ApprovedBy,
							P.IsUpdate = @IsUpdate,
							P.IsNewRow = 1
						FROM @PersonCalendarLog P
						JOIN PersonCalendar PC ON P.PersonId IS NULL AND PC.SeriesId = P.SeriesId
                END
            ELSE 
                BEGIN

				INSERT INTO @PersonCalendarLog
					SELECT  ROW_NUMBER() OVER(ORDER BY @StartDate),
							@StartDate,
							@EndDate,
							P.PersonId,
							P.DayOff,
							P.ActualHours,
							P.IsSeries,
							P.TimeTypeId,
							P.SubstituteDate,
							P.Description,
							0,
							P.ApprovedBy,
							P.SeriesId,
							2,
							0
					FROM dbo.PersonCalendar P
                    WHERE   PersonId = @PersonId
                            AND Date = @StartDate

                    DELETE  dbo.PersonCalendar
                    WHERE   PersonId = @PersonId
                            AND Date BETWEEN @StartDate AND @EndDate
                END
	
	--Delete TimeOff(other than holiday administrative timetype) TimeEntries for all type of persons(w2salary/w2hourly/etc.) if there is no entry in PersonCalendar
            DELETE  TEH
            FROM    dbo.TimeEntry TE
                    INNER JOIN dbo.ChargeCode CC ON ((TE.ChargeCodeDate BETWEEN @StartDate AND @EndDate) OR (TE.ChargeCodeDate BETWEEN @SeriesStartDate AND @SeriesEndDate))
                                                    AND TE.PersonId = @PersonId
                                                    AND TE.ChargeCodeId = CC.Id
                    INNER JOIN dbo.TimeType TT ON TT.TimeTypeId = CC.TimeTypeId
                                                  AND TT.IsAdministrative = 1
                                                  AND TT.TimeTypeId <> @HolidayTimeTypeId
                    INNER JOIN dbo.TimeEntryHours TEH ON TEH.TimeEntryId = TE.TimeEntryId
                    INNER JOIN dbo.Calendar C ON C.Date = TE.ChargeCodeDate
                                                 AND C.DayOff = 0
                    LEFT JOIN dbo.PersonCalendar PC ON PC.Date = TE.ChargeCodeDate
                                                       AND PC.TimeTypeId = CC.TimeTypeId
                                                       AND PC.PersonId = TE.PersonId
                                                       AND PC.DayOff = 1
            WHERE   PC.Date IS NULL

            DELETE  TE
            FROM    dbo.TimeEntry TE
                    INNER JOIN dbo.ChargeCode CC ON ((TE.ChargeCodeDate BETWEEN @StartDate AND @EndDate) OR (TE.ChargeCodeDate BETWEEN @SeriesStartDate AND @SeriesEndDate))
                                                    AND TE.PersonId = @PersonId
                                                    AND TE.ChargeCodeId = CC.Id
                    INNER JOIN dbo.TimeType TT ON TT.TimeTypeId = CC.TimeTypeId
                                                  AND TT.IsAdministrative = 1
                                                  AND TT.TimeTypeId <> @HolidayTimeTypeId
                    INNER JOIN dbo.Calendar C ON C.Date = TE.ChargeCodeDate
                                                 AND C.DayOff = 0
                    LEFT JOIN dbo.PersonCalendar PC ON PC.Date = TE.ChargeCodeDate
                                                       AND PC.TimeTypeId = CC.TimeTypeId
                                                       AND PC.PersonId = TE.PersonId
                                                       AND PC.DayOff = 1
            WHERE   PC.Date IS NULL

	--Update TimeTypeId And Note
            UPDATE  TE
            SET     ChargeCodeId = CC.Id ,
                    Note = PC.Description
            FROM    dbo.PersonCalendar PC
                    INNER JOIN dbo.ChargeCode CC ON PC.Date BETWEEN @StartDate AND @EndDate
                                                    AND PC.PersonId = @PersonId
                                                    AND PC.DayOff = 1
                                                    AND PC.TimeTypeId = CC.TimeTypeId
                    INNER JOIN dbo.TimeEntry TE ON TE.PersonId = PC.PersonId
                                                   AND TE.ChargeCodeDate = PC.Date
                    INNER JOIN dbo.ChargeCode TECC ON TE.ChargeCodeId = TECC.Id
                    INNER JOIN dbo.TimeType TT ON TT.TimeTypeId = TECC.TimeTypeId
                                                  AND TT.IsAdministrative = 1
                                                  AND ( TE.ChargeCodeId <> CC.Id
                                                        OR TE.Note <> PC.Description
                                                      )

	--Update ActualHours
            UPDATE  TEH
            SET     ActualHours = PC.ActualHours
            FROM    dbo.PersonCalendar PC
                    INNER JOIN dbo.ChargeCode CC ON PC.Date BETWEEN @StartDate AND @EndDate
                                                    AND PC.PersonId = @PersonId
                                                    AND PC.DayOff = 1
                                                    AND PC.TimeTypeId = CC.TimeTypeId
                    INNER JOIN dbo.TimeEntry TE ON TE.PersonId = PC.PersonId
                                                   AND TE.ChargeCodeId = CC.Id
                                                   AND TE.ChargeCodeDate = PC.Date
                    INNER JOIN dbo.TimeEntryHours TEH ON TE.TimeEntryId = TEH.TimeEntryId
                                                         AND PC.ActualHours <> TEH.ActualHours

	--Insert TimeEntries only for w2salaried/w2hourly persons, if there is entry in PersonCalendar.
            INSERT  INTO [dbo].[TimeEntry]
                    ( [PersonId] ,
                      [ChargeCodeId] ,
                      [ChargeCodeDate] ,
                      [Note] ,
                      [ForecastedHours] ,
                      [IsCorrect] ,
                      [IsAutoGenerated]
		            )
                    SELECT DISTINCT
                            P.PersonId ,
                            CC.Id ,
                            PC.[Date] ,
                            PC.Description ,
                            0 ,
                            1 ,
                            1 --Here it is Auto generated.
                    FROM    dbo.PersonCalendar PC
                            INNER JOIN dbo.Calendar C ON PC.Date BETWEEN @StartDate AND @EndDate
                                                         AND PC.PersonId = @PersonId
                                                         AND PC.DayOff = 1
                                                         AND C.Date = PC.Date
                                                         AND C.DayOff <> 1
                            INNER JOIN dbo.Person P ON P.PersonId = PC.PersonId
                                                       AND P.IsStrawman = 0
                            INNER JOIN dbo.Pay pay ON p.PersonId = pay.Person
                                                      AND pay.Timescale IN (@W2SalaryId,@W2HourlyId)
                                                      AND PC.Date BETWEEN pay.StartDate
                                                              AND
                                                              ( CASE
                                                              WHEN p.TerminationDate IS NOT NULL
                                                              AND pay.EndDate
                                                              - 1 > p.TerminationDate
                                                              THEN p.TerminationDate
                                                              ELSE pay.EndDate
                                                              - 1
                                                              END )
                            INNER JOIN dbo.ChargeCode CC ON CC.TimeTypeId = PC.TimeTypeId
                            LEFT JOIN dbo.TimeEntry TE ON TE.PersonId = P.PersonId
                                                          AND TE.ChargeCodeId = CC.Id
                                                          AND TE.ChargeCodeDate = PC.Date
                    WHERE   TE.TimeEntryId IS NULL

            INSERT  INTO [dbo].[TimeEntryHours]
                    ( [TimeEntryId] ,
                      [ActualHours] ,
                      [CreateDate] ,
                      [ModifiedDate] ,
                      [ModifiedBy] ,
                      [IsChargeable] ,
                      [ReviewStatusId]
						
                    )
                    SELECT  TE.TimeEntryId ,
                            CASE PC.TimeTypeId
                              WHEN @HolidayTimeTypeId THEN 8
                              ELSE @ActualHours
                            END ,
                            @CurrentPMTime ,
                            @CurrentPMTime ,
                            @ModifiedBy ,
                            0 ,--Non Billable
                            CASE WHEN PC.IsFromTimeEntry <> 1 THEN 2
                                 ELSE 1
                            END --Inserting timeEntries with Approved Status.
                    FROM    dbo.PersonCalendar PC
                            INNER JOIN dbo.Calendar C ON PC.Date BETWEEN @StartDate AND @EndDate
                                                         AND PC.PersonId = @PersonId
                                                         AND PC.DayOff = 1
                                                         AND C.Date = PC.Date
                                                         AND C.DayOff <> 1
                            INNER JOIN dbo.Person P ON P.PersonId = PC.PersonId
                                                       AND P.IsStrawman = 0
                            INNER JOIN dbo.Pay pay ON p.PersonId = pay.Person
                                                      AND pay.Timescale IN (@W2SalaryId,@W2HourlyId)
                                                      AND PC.Date BETWEEN pay.StartDate
                                                              AND
                                                              ( CASE
                                                              WHEN p.TerminationDate IS NOT NULL
                                                              AND pay.EndDate
                                                              - 1 > p.TerminationDate
                                                              THEN p.TerminationDate
                                                              ELSE pay.EndDate
                                                              - 1
                                                              END )
                            INNER JOIN dbo.ChargeCode CC ON PC.TimeTypeId = CC.TimeTypeId
                            INNER JOIN dbo.TimeEntry TE ON TE.PersonId = PC.PersonId
                                                           AND TE.ChargeCodeId = CC.Id
                                                           AND TE.ChargeCodeDate = PC.Date
                            LEFT JOIN dbo.TimeEntryHours TEH ON TEH.TimeEntryId = TE.TimeEntryId
                    WHERE   TEH.TimeEntryId IS NULL
					--NEW_VALUES,OLD_VALUES CTE FOR ACTIVITY XML 
					
					EXEC dbo.SessionLogUnprepare
					EXEC dbo.SessionLogPrepare @UserLogin = @UserLogin

					;WITH NEW_VALUES AS
					(
						SELECT	P.Id,
								CONVERT(NVARCHAR(10), P.StartDate, 101) AS [StartDate],
								CONVERT(NVARCHAR(10), P.EndDate, 101) AS [EndDate],	
								P.PersonId,
								per1.LastName+', '+per1.FirstName AS PersonName,
								CAST(P.ActualHours as decimal(20,2)) AS ActualHours,
								CASE WHEN P.IsFromTimeEntry = 1 THEN 'YES' ELSE 'NO' END IsFromTimeEntry,
								P.TimeTypeId,
								TT.Name AS TimeTypeName,
								P.Description AS Notes,
								P.ApprovedBy AS ApprovedPersonId,
								ISNULL(per2.LastName+', '+per2.FirstName,'') AS ApprovedBy,
								P.SeriesId
						FROM @PersonCalendarLog P
						LEFT JOIN dbo.Person per1 ON per1.PersonId = P.PersonId
						LEFT JOIN dbo.TimeType TT ON TT.TimeTypeId = P.TimeTypeId
						LEFT JOIN dbo.Person per2 ON per2.PersonId = P.ApprovedBy
						WHERE P.IsNewRow = 1 			
					),

					OLD_VALUES AS
					(
						SELECT	P.Id,
								CONVERT(NVARCHAR(10), P.StartDate, 101) AS [StartDate],
								CONVERT(NVARCHAR(10), P.EndDate, 101) AS [EndDate],	
								P.PersonId,
								per1.LastName+', '+per1.FirstName AS PersonName,
								CAST(P.ActualHours as decimal(20,2)) AS ActualHours,
								CASE WHEN P.IsFromTimeEntry = 1 THEN 'YES' ELSE 'NO' END IsFromTimeEntry,
								P.TimeTypeId,
								TT.Name AS TimeTypeName,
								P.Description AS Notes,
								P.ApprovedBy AS ApprovedPersonId,
								ISNULL(per2.LastName+', '+per2.FirstName,'') AS ApprovedBy,
								P.SeriesId
						FROM @PersonCalendarLog P
						LEFT JOIN dbo.Person per1 ON per1.PersonId = P.PersonId
						LEFT JOIN dbo.TimeType TT ON TT.TimeTypeId = P.TimeTypeId
						LEFT JOIN dbo.Person per2 ON per2.PersonId = P.ApprovedBy
						WHERE P.IsNewRow = 0 			
					)
		-- Log an activity
	INSERT INTO dbo.UserActivityLog
	            (ActivityTypeID,
	             SessionID,
	             SystemUser,
	             Workstation,
	             ApplicationName,
	             UserLogin,
	             PersonID,
	             LastName,
	             FirstName,
				 Data,
	             LogData,
	             LogDate)
	SELECT  CASE
	           WHEN d.PersonId IS NULL THEN 3
	           WHEN i.PersonId IS NULL THEN 5
	           ELSE 4
	       END as ActivityTypeID,
	       l.SessionID,
	       l.SystemUser,
	       l.Workstation,
	       l.ApplicationName,
	       l.UserLogin,
	       l.PersonID,
	       l.LastName,
	       l.FirstName,
		   Data = CONVERT(NVARCHAR(MAX),(SELECT *
					    FROM NEW_VALUES
					         FULL JOIN OLD_VALUES ON NEW_VALUES.PersonId = OLD_VALUES.PersonId AND NEW_VALUES.Id = OLD_VALUES.Id
			           WHERE (NEW_VALUES.PersonId = ISNULL(i.PersonId, d.PersonId) OR OLD_VALUES.PersonId = ISNULL(i.PersonId, d.PersonId)) AND (NEW_VALUES.Id = ISNULL(i.Id, d.Id) OR OLD_VALUES.Id = ISNULL(i.Id, d.Id))
					  FOR XML AUTO, ROOT('PersonCalendar'))),
		   LogData = (SELECT 
						 NEW_VALUES.[StartDate] 
						,NEW_VALUES.[EndDate]
						,NEW_VALUES.PersonId
						,NEW_VALUES.PersonName
						,NEW_VALUES.ActualHours
						,NEW_VALUES.IsFromTimeEntry
						,NEW_VALUES.TimeTypeId 
						,NEW_VALUES.TimeTypeName
						,NEW_VALUES.Notes 
						,NEW_VALUES.ApprovedPersonId 
						,NEW_VALUES.ApprovedBy
						,NEW_VALUES.SeriesId
						,OLD_VALUES.[StartDate] 
						,OLD_VALUES.[EndDate]
						,OLD_VALUES.PersonId
						,OLD_VALUES.PersonName
						,OLD_VALUES.ActualHours
						,OLD_VALUES.IsFromTimeEntry
						,OLD_VALUES.TimeTypeId 
						,OLD_VALUES.TimeTypeName
						,OLD_VALUES.Notes 
						,OLD_VALUES.ApprovedPersonId 
						,OLD_VALUES.ApprovedBy
						,OLD_VALUES.SeriesId
					  FROM NEW_VALUES
					         FULL JOIN OLD_VALUES ON NEW_VALUES.PersonId = OLD_VALUES.PersonId AND NEW_VALUES.Id = OLD_VALUES.Id
			            WHERE (NEW_VALUES.PersonId = ISNULL(i.PersonId , d.PersonId ) OR OLD_VALUES.PersonId = ISNULL(i.PersonId , d.PersonId)) AND (NEW_VALUES.Id = ISNULL(i.Id , d.Id ) OR OLD_VALUES.Id = ISNULL(i.Id , d.Id))
					FOR XML AUTO, ROOT('PersonCalendar'), TYPE),
					@CurrentPMTime
	  FROM NEW_VALUES AS i
	       FULL JOIN OLD_VALUES AS d ON i.PersonId = d.PersonId AND i.Id = d.Id 
	       INNER JOIN dbo.SessionLogData AS l ON l.SessionID = @@SPID
	  WHERE  i.StartDate <> d.StartDate
	  OR i.EndDate <> d.EndDate
	  OR i.ActualHours <> d.ActualHours
	  OR i.TimeTypeId <> i.TimeTypeId
	  OR ISNULL(i.Notes, '') <> ISNULL(d.Notes, '')

            COMMIT TRANSACTION tran_SaveTimeOff
        END TRY
        BEGIN CATCH
            ROLLBACK TRANSACTION tran_SaveTimeOff

            DECLARE @Error NVARCHAR(2000)
            SET @Error = ERROR_MESSAGE()

            RAISERROR(@Error, 16, 1)
        END CATCH
			EXEC dbo.SessionLogUnprepare
    END

