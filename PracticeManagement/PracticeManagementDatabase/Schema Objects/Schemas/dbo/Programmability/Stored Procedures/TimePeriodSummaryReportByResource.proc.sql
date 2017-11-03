﻿-- =========================================================================
-- Author:		Sainath.CH
-- Create date: 03-05-2012
-- Updated by : Srinivas.M
-- Update Date: 06-19-2012
-- Description:  Time Entries grouped by Resource for a particular period.
-- =========================================================================
CREATE PROCEDURE [dbo].[TimePeriodSummaryReportByResource]
	(
	  @StartDate DATETIME ,
	  @EndDate DATETIME ,
	  @IncludePersonsWithNoTimeEntries BIT ,
	  @PersonTypes NVARCHAR(MAX) = NULL ,
	  @TitleIds NVARCHAR(MAX) = NULL ,
	  @TimeScaleNamesList XML = NULL,
	  @PersonStatusIds NVARCHAR(MAX) = NULL,
	  @PersonDivisionIds NVARCHAR(MAX) = NULL
	)
AS 
	BEGIN
		SET NOCOUNT ON;
		DECLARE @StartDateLocal DATETIME ,
			@EndDateLocal DATETIME ,
			@NOW DATE ,
			@HolidayTimeType INT ,
			@FutureDate DATETIME

		SELECT @StartDateLocal = CONVERT(DATE, @StartDate), 
			   @EndDateLocal = CONVERT(DATE, @EndDate),
			   @NOW = dbo.GettingPMTime(GETUTCDATE()),
			   @HolidayTimeType = dbo.GetHolidayTimeTypeId(),
			   @FutureDate = dbo.GetFutureDate()


		DECLARE @TimeScaleNames TABLE ( Name NVARCHAR(1024) )
	
		INSERT  INTO @TimeScaleNames
				SELECT  ResultString
				FROM    [dbo].[ConvertXmlStringInToStringTable](@TimeScaleNamesList)

		DECLARE @DivisionIds TABLE ( Id NVARCHAR(20))

		INSERT INTO @DivisionIds
				SELECT ResultString
				FROM	[dbo].[ConvertXmlStringInToStringTable](@PersonDivisionIds)

	-- Get person level Default hours in between the StartDate and EndDate
	--1.Day should not be company holiday and also not converted to substitute day.
	--2.day should be company holiday and it should be taken as a substitute holiday.

						SELECT   Pc.Personid ,
				                SUM(CASE WHEN PC.Date < @NOW THEN 1
										 ELSE 0 END) * 8 AS  DefaultHoursUntilToday,
								( COUNT(PC.Date) * 8 ) AS DefaultHours --Estimated working hours per day is 8.
					   INTO #PersonDefaultHoursWithInPeriod
					   FROM     ( SELECT    CAL.Date ,
											P.PersonId ,
											CAL.DayOff AS CompanyDayOff ,
											PCAL.TimeTypeId ,
											PCAL.SubstituteDate
								  FROM      dbo.Calendar AS CAL
											INNER JOIN dbo.v_PersonHistory AS P ON CAL.Date >= P.HireDate
															  AND CAL.Date <= ISNULL(P.TerminationDate,
															  @FutureDate)
											LEFT JOIN dbo.PersonCalendar AS PCAL ON PCAL.Date = CAL.Date
															  AND PCAL.PersonId = P.PersonId
								) AS PC
					   WHERE    PC.Date BETWEEN @StartDateLocal
										AND  @EndDateLocal												
								AND DATENAME(weekday,PC.Date) != 'Saturday' AND DATENAME(weekday,PC.Date) != 'Sunday'
					   GROUP BY PC.PersonId
					 
				
				      SELECT DISTINCT
								PTSH.PersonId
					   INTO #ActivePersonsInSelectedRange
					   FROM     dbo.PersonStatusHistory PTSH
					   WHERE    PTSH.StartDate <= @EndDateLocal
								AND @StartDateLocal <=  ISNULL(PTSH.EndDate,@FutureDate)
								AND PTSH.PersonStatusId IN (1,5) --ACTIVE STATUS
								
					
				    
				  
					   SELECT   P.PersonId ,
								MAX(pa.StartDate) AS StartDate
					   INTO #PersonPayDuringSelectedRange
					   FROM     dbo.Person AS P
								LEFT JOIN dbo.Pay pa ON pa.Person = P.PersonId 
														AND pa.StartDate <= @EndDateLocal AND (ISNULL(pa.EndDate, @FutureDate) - 1) >= @StartDateLocal
					   WHERE    P.IsStrawman = 0
					   GROUP BY P.PersonId

					
					 
					
					   SELECT   P.PersonId ,
								MAX(pa.StartDate) AS StartDate
					   INTO #PersonPayToday
					   FROM     dbo.Person AS P
								LEFT JOIN dbo.Pay AS pa ON pa.Person = P.PersonId 
														AND @NOW BETWEEN pa.StartDate AND (ISNULL(pa.EndDate, @FutureDate) - 1)
					   WHERE    P.IsStrawman = 0
					   GROUP BY P.PersonId
					
					SELECT t.* INTO #PersonWithPay 
					FROM
					 (
					  SELECT PPDTP.PersonId,
					         ISNULL(TS.Name,'') AS Timescale
					  FROM	#PersonPayDuringSelectedRange AS PPDTP
					  LEFT JOIN dbo.Pay AS pa ON pa.Person = PPDTP.PersonId  AND pa.StartDate = PPDTP.StartDate
					  LEFT JOIN dbo.Timescale TS ON PA.Timescale = TS.TimescaleId
					  WHERE PPDTP.StartDate IS NOT NULL
					  UNION 
					  SELECT PPT.PersonId,
					         ISNULL(TS.Name,'') AS Timescale
					  FROM	 #PersonPayDuringSelectedRange AS PPDTP
					  INNER JOIN #PersonPayToday AS PPT ON PPDTP.PersonId = PPT.PersonId
					  LEFT JOIN dbo.Pay AS pa ON pa.Person = PPT.PersonId  AND pa.StartDate = PPT.StartDate
					  LEFT JOIN dbo.Timescale TS ON PA.Timescale = TS.TimescaleId
					  WHERE PPDTP.StartDate IS NULL
					 ) as t

					 
			SELECT  P.PersonId ,
					P.LastName ,
					ISNULL(P.PreferredFirstName,P.FirstName) AS FirstName,
					S.TitleId ,
					S.Title,
					P.IsOffshore ,
					P.EmployeeNumber,
					ISNULL(Data.BillableHours, 0) AS BillableHours ,
					ISNULL(Data.BillableHoursUntilToday, 0) AS BillableHoursUntilToday ,
					ISNULL(Data.ProjectNonBillableHours, 0) AS ProjectNonBillableHours ,
					ISNULL(Data.BusinessDevelopmentHours, 0) AS BusinessDevelopmentHours ,
					ISNULL(Data.InternalHours, 0) AS InternalHours ,
					ISNULL(Data.PTOHours, 0) AS PTOHours ,
					ISNULL(Data.HolidayHours, 0) AS HolidayHours ,
					ISNULL(Data.JuryDutyHours, 0) AS JuryDutyHours ,
					ISNULL(Data.BereavementHours, 0) AS BereavementHours ,
					ISNULL(Data.ORTHours, 0) AS ORTHours ,
					ISNULL(Data.UnpaidHours, 0) AS UnpaidHours ,
					ISNULL(Data.SickOrSafeLeaveHours, 0) AS SickOrSafeLeaveHours ,
					ISNULL(CASE WHEN ISNULL(PDH.DefaultHoursUntilToday, 0) = 0
									  THEN 0
									  ELSE ( Data.BillableHoursUntilToday * 100 )
										   / PDH.DefaultHoursUntilToday
								 END, 0) AS BillableUtilizationPercent ,
					PCP.Timescale,
					PS.PersonStatusId AS 'PersonStatusId',
					PS.Name AS 'PersonStatusName',
					P.DivisionId AS 'DivisionId',
					ISNULL(CAST(PDH.DefaultHoursUntilToday AS INT),0) AS AvailableHours,
					ISNULL(CAST(PDH.DefaultHoursUntilToday AS INT),0) AS AvailableHoursUntilToday
			FROM    ( SELECT    TE.PersonId ,
								ROUND(SUM(CASE WHEN TEH.IsChargeable = 1
													AND Pro.ProjectNumber != 'P031000'
											   THEN TEH.ActualHours
											   ELSE 0
										  END), 2) AS BillableHours ,
								ROUND(SUM(CASE WHEN ( TEH.IsChargeable = 1 AND Pro.ProjectNumber != 'P031000'
												  AND TE.ChargeCodeDate < @NOW
												) THEN TEH.ActualHours
										   ELSE 0
									  END), 2) AS BillableHoursUntilToday ,
								ROUND(SUM(CASE WHEN TEH.IsChargeable = 0
													AND CC.TimeEntrySectionId = 1
													AND Pro.ProjectNumber != 'P031000'
											   THEN TEH.ActualHours
											   ELSE 0
										  END), 2) AS ProjectNonBillableHours ,
								ROUND(SUM(CASE WHEN (CC.TimeEntrySectionId = 2 OR PRO.IsBusinessDevelopment = 1) -- Added this condition as part of PP29 changes by Nick.
											   THEN TEH.ActualHours
											   ELSE 0
										  END), 2) AS BusinessDevelopmentHours ,
								ROUND(SUM(CASE WHEN (CC.TimeEntrySectionId = 3 AND Pro.IsBusinessDevelopment <> 1) -- Added this condition as part of PP29 changes by Nick.
													OR Pro.ProjectNumber = 'P031000'
											   THEN TEH.ActualHours
											   ELSE 0
										  END), 2) AS InternalHours ,
								ROUND(SUM(CASE WHEN CC.TimeEntrySectionId = 4 
													AND TT.Code = 'W9310'
											   THEN TEH.ActualHours
											   ELSE 0
										  END), 2) AS PTOHours ,
								ROUND(SUM(CASE WHEN CC.TimeEntrySectionId = 4 
													AND TT.Code IN ('W9320','W9610')
											  THEN TEH.ActualHours
									          ELSE 0
									     END), 2) AS HolidayHours ,
								ROUND(SUM(CASE WHEN CC.TimeEntrySectionId = 4 
													AND TT.Code = 'W9340'
									THEN TEH.ActualHours
									ELSE 0
									     END), 2) AS BereavementHours ,
								ROUND(SUM(CASE WHEN CC.TimeEntrySectionId = 4 
													AND TT.Code = 'W9330'
									THEN TEH.ActualHours
									ELSE 0
									     END), 2) AS JuryDutyHours ,
								ROUND(SUM(CASE WHEN CC.TimeEntrySectionId = 4 
													AND TT.Code = 'W9300'
									THEN TEH.ActualHours
									ELSE 0
									     END), 2) AS ORTHours ,
							    ROUND(SUM(CASE WHEN CC.TimeEntrySectionId = 4 
													AND TT.Code = 'W9350'
									THEN TEH.ActualHours
									ELSE 0
									     END), 2) AS UnpaidHours ,	
								ROUND(SUM(CASE WHEN CC.TimeEntrySectionId = 4 
										AND TT.Code = 'W9311'
									THEN TEH.ActualHours
									ELSE 0
									     END), 2) AS SickOrSafeLeaveHours ,							
								ROUND(SUM(CASE WHEN CC.TimeEntrySectionId = 1
													AND @NOW > TE.ChargeCodeDate
													AND Pro.ProjectNumber != 'P031000'
											   THEN TEH.ActualHours
											   ELSE 0
										  END), 2) AS ActualHours
					  FROM      dbo.TimeEntry TE
								INNER JOIN dbo.TimeEntryHours TEH ON TEH.TimeEntryId = te.TimeEntryId
															  AND TE.ChargeCodeDate BETWEEN @StartDateLocal
															  AND
															  @EndDateLocal
								INNER JOIN dbo.ChargeCode CC ON CC.Id = TE.ChargeCodeId
								INNER JOIN dbo.TimeType TT ON CC.TimeTypeId = TT.TimeTypeId
								INNER JOIN dbo.Project PRO ON PRO.ProjectId = CC.ProjectId
								INNER JOIN dbo.Person P ON P.PersonId = TE.PersonId
								INNER JOIN dbo.PersonStatusHistory PTSH ON PTSH.PersonId = P.PersonId
															  AND TE.ChargeCodeDate BETWEEN PTSH.StartDate
															  AND ISNULL(PTSH.EndDate,@FutureDate)
					  WHERE     TE.ChargeCodeDate <= ISNULL(P.TerminationDate,
															@FutureDate)
								AND ( CC.timeTypeId != @HolidayTimeType
									  OR ( CC.timeTypeId = @HolidayTimeType
										   AND PTSH.PersonStatusId IN (1,5)
										 )
									)
					  GROUP BY  TE.PersonId
					) Data
					FULL JOIN #ActivePersonsInSelectedRange AP ON AP.PersonId = Data.PersonId
					INNER JOIN dbo.Person P ON ( P.PersonId = Data.PersonId
												 OR AP.PersonId = P.PersonId
											   )
											   AND p.IsStrawman = 0
					INNER JOIN dbo.Title S ON S.TitleId = P.TitleId
					INNER JOIN #PersonWithPay PCP ON P.PersonId = PCP.PersonId 
					INNER JOIN [dbo].[PersonStatus] PS ON PS.PersonStatusId = P.PersonStatusId
					LEFT JOIN #PersonDefaultHoursWithInPeriod PDH ON PDH.PersonId = P.PersonId
			WHERE   ( @IncludePersonsWithNoTimeEntries = 1
					  OR ( @IncludePersonsWithNoTimeEntries = 0
						   AND Data.PersonId IS NOT NULL
						 )
					)
					AND ( ( @PersonTypes IS NULL
							OR ( CASE WHEN P.IsOffshore = 1 THEN 'Offshore'
									  ELSE 'Domestic'
								 END ) IN (
							SELECT  ResultString
							FROM    [dbo].[ConvertXmlStringInToStringTable](@PersonTypes) )
						  )
						  AND ( @TitleIds IS NULL
								OR S.TitleId IN (
								SELECT  ResultId
								FROM    dbo.ConvertStringListIntoTable(@TitleIds) )
							  )
						   AND ( @PersonStatusIds IS NULL
								 OR PS.PersonStatusId IN (
								 SELECT  ResultId
								 FROM    dbo.ConvertStringListIntoTable(@PersonStatusIds) )
							  )
						  AND ( @TimeScaleNamesList IS NULL
								OR ( PCP.Timescale ) IN ( 
								  SELECT  Name FROM    @TimeScaleNames )
							  )
						  AND ( @PersonDivisionIds IS NULL
								OR ISNULL(P.DivisionId, '') IN (SELECT Id
																FROM @DivisionIds)
							  )
						)
			ORDER BY P.LastName ,
					P.FirstName


			DROP TABLE #PersonDefaultHoursWithInPeriod
			DROP TABLE #ActivePersonsInSelectedRange
			DROP TABLE #PersonPayDuringSelectedRange
			DROP TABLE #PersonPayToday
			DROP TABLE #PersonWithPay

	END

