﻿CREATE TRIGGER [tr_TT_LogInsert]
    ON [dbo].[TimeTrack]
    AFTER INSERT
AS 
BEGIN
	
	IF EXISTS(SELECT  1 FROM Inserted WHERE IsAutoGenerated = 0)
	BEGIN
		-- Ensure the temporary table exists
		EXEC SessionLogPrepare @UserLogin = NULL


		DECLARE @CurrentPMTime DATETIME 
		SET @CurrentPMTime = dbo.InsertingTime()

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
		SELECT 3 AS ActivityTypeID /* insert only */,
			   l.SessionID,
			   l.SystemUser,
			   l.Workstation,
			   l.ApplicationName,
			   l.UserLogin,
			   l.PersonID,
			   l.LastName,
			   l.FirstName,
			   Data =  CONVERT(NVARCHAR(MAX),(SELECT   1 AS Tag,
								   NULL AS Parent, 
								   NEW_VALUES.[TimeEntryId] AS 'NEW_VALUES!1!TimeEntryId'
								  ,CONVERT(VARCHAR(10), NEW_VALUES.[CreateDate], 101) AS 'NEW_VALUES!1!CreateDate'
								  ,CONVERT(VARCHAR(10), NEW_VALUES.[ModifiedDate], 101) AS 'NEW_VALUES!1!ModifiedDate'
								  ,CAST(NEW_VALUES.[ActualHours] as decimal(20,2)) AS 'NEW_VALUES!1!ActualHours'
								  ,CAST(NEW_VALUES.[ForecastedHours] as decimal(20,2)) AS 'NEW_VALUES!1!ForecastedHours'
								  ,CC.[TimeTypeId] AS 'NEW_VALUES!1!TimeTypeId'
								  ,tType.[Name] AS 'NEW_VALUES!1!TimeTypeName'
								  ,NEW_VALUES.[ModifiedBy] AS 'NEW_VALUES!1!ModifiedBy'
								  ,NEW_VALUES.[Note] AS 'NEW_VALUES!1!Note'
								  ,CASE NEW_VALUES.[IsReviewed] WHEN 1 THEN 'Approved'
										WHEN 0 THEN 'Declined'
										 ELSE 'Pending' END AS 'NEW_VALUES!1!ReviewStatus' 
								  ,CASE NEW_VALUES.[IsCorrect] WHEN 1 THEN 'Correct'
														 ELSE 'InCorrect' END AS 'NEW_VALUES!1!IsCorrect'
								  ,CASE NEW_VALUES.[IsChargeable] WHEN 1 THEN 'Billable'
															ELSE 'Not Billable' END AS 'NEW_VALUES!1!IsBillable'
								  ,CONVERT(VARCHAR(10), NEW_VALUES.[ChargeCodeDate], 101) AS 'NEW_VALUES!1!ChargeCodeDate'
								  ,modp.LastName + ', ' + modp.FirstName AS 'NEW_VALUES!1!ModifiedByName'
								  ,objp.LastName + ', ' + objp.FirstName AS 'NEW_VALUES!1!ObjectName'
								  ,objp.PersonId AS 'NEW_VALUES!1!ObjectPersonId'
								  ,clnt.[name] AS 'NEW_VALUES!1!ClientName'
								  ,clnt.[ClientId] AS 'NEW_VALUES!1!ClientId'
								  ,PG.[Name] AS 'NEW_VALUES!1!ProjectGroupName'
								  ,PG.[GroupId] AS 'NEW_VALUES!1!ProjectGroupId'
								  ,proj.[name] AS 'NEW_VALUES!1!ProjectName'
								  ,proj.ProjectId AS 'NEW_VALUES!1!ProjectId'
								  ,clnt.[Code] + ' - ' + PG.[Code] + ' - ' + proj.ProjectNumber + ' - ' + '01 - ' + tType.Code AS 'NEW_VALUES!1!ChargeCode'
							FROM inserted AS NEW_VALUES
							INNER JOIN dbo.ChargeCode AS CC ON CC.Id = NEW_VALUES.ChargeCodeId
							INNER JOIN dbo.Person AS modp ON modp.PersonId = NEW_VALUES.ModifiedBy
							INNER JOIN dbo.Person AS objp ON objp.PersonId = NEW_VALUES.PersonId
							INNER JOIN dbo.Project AS proj ON proj.ProjectId = CC.ProjectId
							INNER JOIN dbo.Client AS clnt ON CC.ClientId = clnt.ClientId
							INNER JOIN dbo.ProjectGroup AS PG ON PG.GroupId = CC.ProjectGroupId
							INNER JOIN dbo.TimeType AS tType ON tType.TimeTypeId = CC.TimeTypeId
						   WHERE NEW_VALUES.TimeEntryId = i.TimeEntryId
						  FOR XML EXPLICIT, ROOT('TimeEntry'))),
						  LogData = (SELECT   1 AS Tag,
								   NULL AS Parent, 
								   NEW_VALUES.[TimeEntryId] AS 'NEW_VALUES!1!TimeEntryId'
								  ,CONVERT(VARCHAR(10), NEW_VALUES.[CreateDate], 101) AS 'NEW_VALUES!1!CreateDate'
								  ,CONVERT(VARCHAR(10), NEW_VALUES.[ModifiedDate], 101) AS 'NEW_VALUES!1!ModifiedDate'
								  ,NEW_VALUES.[PersonId] AS 'NEW_VALUES!1!PersonId'
								  ,CC.[TimeTypeId] AS 'NEW_VALUES!1!TimeTypeId'
								  ,tType.[Name] AS 'NEW_VALUES!1!TimeTypeName'
								  ,NEW_VALUES.[Note] AS 'NEW_VALUES!1!Note'
								  ,CONVERT(VARCHAR(10), NEW_VALUES.[ChargeCodeDate], 101) AS 'NEW_VALUES!1!ChargeCodeDate'
								  ,objp.LastName + ', ' + objp.FirstName AS 'NEW_VALUES!1!ObjectName'
								  ,CC.[ClientId] AS 'NEW_VALUES!1!ClientId'
								  ,CC.ProjectGroupId AS 'NEW_VALUES!1!ProjectGroupId'
								  ,CC.ProjectId AS 'NEW_VALUES!1!ProjectId'
							FROM inserted AS NEW_VALUES
							INNER JOIN dbo.ChargeCode CC ON CC.Id = NEW_VALUES.ChargeCodeId
							INNER JOIN dbo.Person AS modp ON modp.PersonId = NEW_VALUES.ModifiedBy
							INNER JOIN dbo.Person AS objp ON objp.PersonId = NEW_VALUES.PersonId
							INNER JOIN dbo.TimeType AS tType ON tType.TimeTypeId = CC.TimeTypeId
						   WHERE NEW_VALUES.TimeEntryId = i.TimeEntryId
						  FOR XML EXPLICIT, ROOT('TimeEntry'), TYPE),
				@CurrentPMTime
		  FROM inserted AS i
			   INNER JOIN dbo.SessionLogData AS l ON l.SessionID = @@SPID
	  

	 END
END

