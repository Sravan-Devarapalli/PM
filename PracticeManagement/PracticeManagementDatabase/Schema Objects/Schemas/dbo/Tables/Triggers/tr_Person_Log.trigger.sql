﻿-- =============================================
-- Author:		Anatoliy Lokshin
-- Create date: 10-14-2008
-- Updated by:	Srinivas.M
-- Update date:	05-21-2012
-- Description:	Logs the changes in the dbo.Person table.
-- =============================================
CREATE TRIGGER [dbo].[tr_Person_Log]
ON [dbo].[Person]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	-- Ensure the temporary table exists
	EXEC SessionLogPrepare @UserLogin = NULL
	
	DECLARE @CurrentPMTime DATETIME 
	SET @CurrentPMTime = dbo.InsertingTime()
	
	IF (SELECT COUNT(ISNULL(i.PersonID,d.PersonID)) 
		FROM inserted AS i	
		FULL JOIN deleted AS d ON i.PersonID = d.PersonID 
		WHERE ISNULL(i.IsStrawman, d.IsStrawman) = 0
		) > 0
	BEGIN 

		IF EXISTS (SELECT 1 FROM inserted AS i	
					INNER JOIN deleted AS d ON i.PersonID = d.PersonID 
					WHERE ISNULL(i.PictureFileName,'') <> ISNULL(d.PictureFileName,'')
					OR ISNULL(i.PictureData, CONVERT(VARBINARY, '')) <> ISNULL(d.PictureData, CONVERT(VARBINARY, ''))
				) 
		BEGIN 
			-- Log an activity for person picture change
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
				SELECT CASE
					WHEN d.PictureFileName IS NULL THEN 3
					WHEN i.PictureFileName IS NULL THEN 5
					ELSE 4
				END AS ActivityTypeID,
				l.SessionID,
				l.SystemUser,
				l.Workstation,
				l.ApplicationName,
				l.UserLogin,
				l.PersonID,
				l.LastName,
				l.FirstName,
				Data =  CONVERT(NVARCHAR(MAX),(SELECT NEW_VALUES.PersonId,
									NEW_VALUES.LastName + ','+ NEW_VALUES.FirstName AS Person,
									NEW_VALUES.PictureFileName ,
									OLD_VALUES.PersonId,
									OLD_VALUES.LastName + ','+ OLD_VALUES.FirstName AS Person,
									OLD_VALUES.PictureFileName
							FROM inserted AS NEW_VALUES
									INNER JOIN deleted AS OLD_VALUES ON NEW_VALUES.PersonID = OLD_VALUES.PersonID 
							FOR XML AUTO, ROOT('PersonSkill'))),
				LogData = (SELECT NEW_VALUES.PersonId,
									NEW_VALUES.LastName + ','+ NEW_VALUES.FirstName AS Person,
									NEW_VALUES.PictureFileName,
									OLD_VALUES.PersonId,
									OLD_VALUES.LastName + ','+ OLD_VALUES.FirstName AS Person,
									OLD_VALUES.PictureFileName
							FROM inserted AS NEW_VALUES
									INNER JOIN deleted AS OLD_VALUES ON NEW_VALUES.PersonID = OLD_VALUES.PersonID 
							FOR XML AUTO, ROOT('PersonSkill'), TYPE),
				@CurrentPMTime
				FROM inserted AS i
				INNER JOIN deleted AS d ON i.PersonID = d.PersonID 
				INNER JOIN dbo.SessionLogData AS l ON l.SessionID = @@SPID
				WHERE ISNULL(i.IsStrawman, d.IsStrawman) = 0 
				AND ( ISNULL(i.PictureFileName,'') <> ISNULL(d.PictureFileName,'')
					OR ISNULL(i.PictureData, CONVERT(VARBINARY, '')) <> ISNULL(d.PictureData, CONVERT(VARBINARY, ''))
					)
		END
		ELSE
		BEGIN	
			;WITH NEW_VALUES AS
			(
			SELECT i.PersonId,
				CONVERT(NVARCHAR(10), i.HireDate, 101) AS HireDate,
				CONVERT(NVARCHAR(10), i.TerminationDate, 101) AS TerminationDate,
				i.Alias ,
				p.Name AS DefaultPractice,
				i.FirstName,
				i.LastName,
				s.Name AS PersonStatusName,
				i.EmployeeNumber,
				i.SeniorityId,
				r.Name AS Seniority,
				CASE WHEN i.IsDefaultManager = 1 THEN 'Yes' ELSE 'NO' END AS [IsDefaultManager],
				i.ManagerId,
				CASE WHEN i.ManagerId IS NULL THEN 'Unassigned' ELSE mngr.LastName + ', ' + mngr.FirstName END [ManagerName],
				i.TelephoneNumber,
				i.DivisionId,
				PD.DivisionName,
				i.PayChexID,
				CASE WHEN i.IsOffshore = 1 THEN 'YES' ELSE 'NO' END AS [IsOffshore],
				TR.TerminationReasonId,
				TR.TerminationReason,
				i.RecruiterId,
				recr.LastName + ', ' + recr.FirstName as RecruiterName,
				i.TitleId,
				T.Title,
				i.JobSeekerStatusId,
				JSS.Name AS JobSeekerStatus,
				i.SourceId,
				RM.Name AS SourceName,
				i.TargetedCompanyId,
				RMT.Name AS TargetedCompanyName,
				i.EmployeeReferralId,
				CASE WHEN i.EmployeeReferralId IS NULL THEN 'NO' ELSE  'YES' END AS EmployeeReferral,
				EmpRef.LastName+ ', '+EmpRef.FirstName AS EmployeeReferralName,
				i.CohortAssignmentId,
				CA.Name AS CohortAssignmentName
			FROM inserted AS i
				LEFT JOIN dbo.Practice AS p ON i.DefaultPractice = p.PracticeId
				INNER JOIN dbo.PersonStatus AS s ON i.PersonStatusId = s.PersonStatusId
				LEFT JOIN dbo.Seniority AS r ON i.SeniorityId = r.SeniorityId
				LEFT JOIN dbo.Person as mngr ON mngr.PersonId = i.ManagerId
				LEFT JOIN dbo.Person as recr ON recr.PersonId = i.RecruiterId
				LEFT JOIN dbo.PersonDivision PD ON PD.DivisionId = i.DivisionId
				LEFT JOIN dbo.TerminationReasons TR ON TR.TerminationReasonId = i.TerminationReasonId
				LEFT JOIN dbo.Title AS T ON i.TitleId = T.TitleId
				LEFT JOIN dbo.JobSeekerStatus JSS ON JSS.JobSeekerStatusId = i.JobSeekerStatusId
				LEFT JOIN dbo.RecruitingMetrics RM ON RM.RecruitingMetricsId = i.SourceId
				LEFT JOIN dbo.RecruitingMetrics RMT ON RMT.RecruitingMetricsId = i.TargetedCompanyId
				LEFT JOIN dbo.Person EmpRef ON EmpRef.PersonId = i.EmployeeReferralId
				LEFT JOIN dbo.CohortAssignment CA ON CA.CohortAssignmentId = i.CohortAssignmentId
			WHERE i.IsStrawman = 0 
			),

			OLD_VALUES AS
			(
			SELECT d.PersonId,
				CONVERT(NVARCHAR(10), d.HireDate, 101) AS HireDate,
				CONVERT(NVARCHAR(10), d.TerminationDate, 101) AS TerminationDate,
				d.Alias ,
				p.Name AS DefaultPractice,
				d.FirstName,
				d.LastName,
				s.Name AS PersonStatusName,
				d.EmployeeNumber,
				d.SeniorityId,
				r.Name AS Seniority,
				CASE WHEN d.IsDefaultManager = 1 THEN 'Yes' ELSE 'NO' END AS [IsDefaultManager],
				d.ManagerId,
				CASE WHEN d.ManagerId IS NULL THEN 'Unassigned' ELSE mngr.LastName + ', ' + mngr.FirstName END [ManagerName],
				d.TelephoneNumber,
				d.DivisionId,
				PD.DivisionName,
				d.PayChexID,
				CASE WHEN d.IsOffshore = 1 THEN 'YES' ELSE 'NO' END AS [IsOffshore],
				TR.TerminationReasonId,
				TR.TerminationReason,
				d.RecruiterId,
				recr.LastName + ', ' + recr.FirstName as RecruiterName,
				d.TitleId,
				T.Title,
				d.JobSeekerStatusId,
				JSS.Name AS JobSeekerStatus,
				d.SourceId,
				RM.Name AS SourceName,
				d.TargetedCompanyId,
				RMT.Name AS TargetedCompanyName,
				d.EmployeeReferralId,
				CASE WHEN d.EmployeeReferralId IS NULL THEN 'NO' ELSE  'YES' END AS EmployeeReferral,
				EmpRef.LastName+ ', '+EmpRef.FirstName AS EmployeeReferralName,
				d.CohortAssignmentId,
				CA.Name AS CohortAssignmentName
			FROM deleted AS d
				LEFT JOIN dbo.Practice AS p ON d.DefaultPractice = p.PracticeId
				INNER JOIN dbo.PersonStatus AS s ON d.PersonStatusId = s.PersonStatusId
				LEFT JOIN dbo.Seniority AS r ON d.SeniorityId = r.SeniorityId
				LEFT JOIN dbo.Person as mngr ON mngr.PersonId = d.ManagerId
				LEFT JOIN dbo.Person as recr ON recr.PersonId = d.RecruiterId
				LEFT JOIN dbo.PersonDivision PD ON PD.DivisionId = d.DivisionId
				LEFT JOIN dbo.TerminationReasons TR ON TR.TerminationReasonId = d.TerminationReasonId
				LEFT JOIN dbo.Title AS T ON d.TitleId = T.TitleId
				LEFT JOIN dbo.JobSeekerStatus JSS ON JSS.JobSeekerStatusId = d.JobSeekerStatusId
				LEFT JOIN dbo.RecruitingMetrics RM ON RM.RecruitingMetricsId = d.SourceId
				LEFT JOIN dbo.RecruitingMetrics RMT ON RMT.RecruitingMetricsId = d.TargetedCompanyId
				LEFT JOIN dbo.Person EmpRef ON EmpRef.PersonId = d.EmployeeReferralId
				LEFT JOIN dbo.CohortAssignment CA ON CA.CohortAssignmentId = d.CohortAssignmentId
			WHERE d.IsStrawman = 0
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
			SELECT CASE
				WHEN d.PersonID IS NULL THEN 3
				WHEN i.PersonID IS NULL THEN 5
				ELSE 4
			END AS ActivityTypeID,
			l.SessionID,
			l.SystemUser,
			l.Workstation,
			l.ApplicationName,
			l.UserLogin,
			l.PersonID,
			l.LastName,
			l.FirstName,
			Data =  CONVERT(NVARCHAR(MAX),(SELECT *
						FROM NEW_VALUES
								FULL JOIN OLD_VALUES ON NEW_VALUES.PersonID = OLD_VALUES.PersonID
						WHERE NEW_VALUES.PersonID = ISNULL(i.PersonId, d.PersonID) OR OLD_VALUES.PersonID = ISNULL(i.PersonId, d.PersonID)
						FOR XML AUTO, ROOT('Person'))),
			LogData = (SELECT NEW_VALUES.PersonId,
								NEW_VALUES.HireDate,
								NEW_VALUES.TerminationDate,
								NEW_VALUES.EmployeeNumber,
								NEW_VALUES.IsDefaultManager,
								NEW_VALUES.TelephoneNumber,
								OLD_VALUES.PersonId,
								OLD_VALUES.HireDate,
								OLD_VALUES.TerminationDate,
								OLD_VALUES.EmployeeNumber,
								OLD_VALUES.IsDefaultManager,
								OLD_VALUES.TelephoneNumber
						FROM NEW_VALUES
								FULL JOIN OLD_VALUES ON NEW_VALUES.PersonID = OLD_VALUES.PersonID
						WHERE NEW_VALUES.PersonID = ISNULL(i.PersonId, d.PersonID) OR OLD_VALUES.PersonID = ISNULL(i.PersonId, d.PersonID)
						FOR XML AUTO, ROOT('Person'), TYPE),
			@CurrentPMTime
			FROM inserted AS i
			FULL JOIN deleted AS d ON i.PersonID = d.PersonID 
			INNER JOIN dbo.SessionLogData AS l ON l.SessionID = @@SPID
			WHERE ISNULL(i.IsStrawman, d.IsStrawman) = 0 
			AND (
				ISNULL(i.HireDate,'') <> ISNULL(d.HireDate,'')
				OR ISNULL(i.TerminationDate,'') <> ISNULL(d.TerminationDate,'')
				OR ISNULL(i.Alias,'') <> ISNULL(d.Alias,'')
				OR ISNULL(i.DefaultPractice,'') <> ISNULL(d.DefaultPractice,'')
				OR ISNULL(i.FirstName,'') <> ISNULL(d.FirstName,'')
				OR ISNULL(i.LastName,'') <> ISNULL(d.LastName,'')
				OR ISNULL(i.PersonStatusId,'') <> ISNULL(d.PersonStatusId,'')
				OR ISNULL(i.EmployeeNumber,'') <> ISNULL(d.EmployeeNumber,'')
				OR ISNULL(i.SeniorityId,0) <> ISNULL(d.SeniorityId,0)
				OR ISNULL(i.IsDefaultManager,'') <> ISNULL(d.IsDefaultManager,'')
				OR ISNULL(i.ManagerId,0) <> ISNULL(d.ManagerId,0)
				OR ISNULL(i.TelephoneNumber,'') <> ISNULL(d.TelephoneNumber,'')
				OR ISNULL(i.DivisionId,0) <> ISNULL(d.DivisionId,0)
				OR ISNULL(i.PayChexID,'') <> ISNULL(d.PayChexID,'')
				OR ISNULL(i.IsOffshore,'') <> ISNULL(d.IsOffshore,'')
				OR ISNULL(i.TerminationReasonId, -1) <> ISNULL(d.TerminationReasonId, -1)
				OR ISNULL(i.[RecruiterId], -1) <> ISNULL(d.[RecruiterId], -1)
				OR ISNULL(i.[TitleId], -1) <> ISNULL(d.[TitleId], -1)
				OR ISNULL(i.JobSeekerStatusId, -1) <> ISNULL(d.JobSeekerStatusId, -1)
				OR ISNULL(i.SourceId, -1) <> ISNULL(d.SourceId, -1)
				OR ISNULL(i.TargetedCompanyId, -1) <> ISNULL(d.TargetedCompanyId, -1)
				OR ISNULL(i.EmployeeReferralId, -1) <> ISNULL(d.EmployeeReferralId, -1)
				OR ISNULL(i.CohortAssignmentId, -1) <> ISNULL(d.CohortAssignmentId, -1)		
			)
		END
	END

	IF (SELECT COUNT(ISNULL(i.PersonID,d.PersonID)) 
		FROM inserted AS i	
		FULL JOIN deleted AS d ON i.PersonID = d.PersonID 
		WHERE ISNULL(i.IsStrawman, d.IsStrawman) = 1
		) > 0
	BEGIN 

		;WITH NEW_VALUES AS
		(
			SELECT i.PersonId,
					CONVERT(NVARCHAR(10), i.HireDate, 101) AS [HireDate],
					CONVERT(NVARCHAR(10), i.TerminationDate, 101) AS [TerminationDate],
					i.FirstName AS [Skill],
					i.LastName AS [Role],
					s.Name AS PersonStatusName,
					i.EmployeeNumber
			FROM inserted AS i
					INNER JOIN dbo.PersonStatus AS s ON i.PersonStatusId = s.PersonStatusId
			WHERE i.IsStrawman = 1
		),
		OLD_VALUES AS
		(
			SELECT d.PersonId,
					CONVERT(NVARCHAR(10), d.HireDate, 101) AS [HireDate],
					CONVERT(NVARCHAR(10), d.TerminationDate, 101) AS [TerminationDate],
					d.FirstName AS [Skill],
					d.LastName AS [Role],
					s.Name AS PersonStatusName,
					d.EmployeeNumber
			FROM deleted AS d
					INNER JOIN dbo.PersonStatus AS s ON d.PersonStatusId = s.PersonStatusId
			WHERE d.IsStrawman = 1
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
		SELECT CASE
					WHEN d.PersonID IS NULL THEN 3
					WHEN i.PersonID IS NULL THEN 5
					ELSE 4
				END AS ActivityTypeID,
				l.SessionID,
				l.SystemUser,
				l.Workstation,
				l.ApplicationName,
				l.UserLogin,
				l.PersonID,
				l.LastName,
				l.FirstName,
				Data =  CONVERT(NVARCHAR(MAX),(SELECT *
							FROM NEW_VALUES
									FULL JOIN OLD_VALUES ON NEW_VALUES.PersonID = OLD_VALUES.PersonID
							WHERE NEW_VALUES.PersonID = ISNULL(i.PersonId, d.PersonID) OR OLD_VALUES.PersonID = ISNULL(i.PersonId, d.PersonID)
							FOR XML AUTO, ROOT('Strawman'))),
				LogData = (SELECT NEW_VALUES.PersonId,
									NEW_VALUES.HireDate,
									NEW_VALUES.TerminationDate,
									NEW_VALUES.EmployeeNumber,
									OLD_VALUES.PersonId,
									OLD_VALUES.HireDate,
									OLD_VALUES.TerminationDate,
									OLD_VALUES.EmployeeNumber
							FROM NEW_VALUES
									FULL JOIN OLD_VALUES ON NEW_VALUES.PersonID = OLD_VALUES.PersonID
							WHERE NEW_VALUES.PersonID = ISNULL(i.PersonId, d.PersonID) OR OLD_VALUES.PersonID = ISNULL(i.PersonId, d.PersonID)
							FOR XML AUTO, ROOT('Strawman'), TYPE),
				@CurrentPMTime
 		FROM inserted AS i
				FULL JOIN deleted AS d ON i.PersonID = d.PersonID 
				INNER JOIN dbo.SessionLogData AS l ON l.SessionID = @@SPID
		WHERE ISNULL(i.IsStrawman, d.IsStrawman) = 1 
		AND (
				ISNULL(i.HireDate,'') <> ISNULL(d.HireDate,'')
				OR ISNULL(i.TerminationDate,'') <> ISNULL(d.TerminationDate,'')
				OR ISNULL(i.FirstName,'') <> ISNULL(d.FirstName,'')
				OR ISNULL(i.LastName,'') <> ISNULL(d.LastName,'')
				OR ISNULL(i.PersonStatusId,'') <> ISNULL(d.PersonStatusId,'')
				OR ISNULL(i.EmployeeNumber,'') <> ISNULL(d.EmployeeNumber,'')
			)
			
	END

	IF EXISTS (SELECT 1
				FROM inserted i)
	BEGIN
		DECLARE @insertTime DATETIME
		SELECT @insertTime = dbo.InsertingTime()

		INSERT INTO PersonHistory([PersonId]
								  ,[HireDate]
								  ,[TerminationDate]
								  ,[Alias]
								  ,[DefaultPractice]
								  ,[FirstName]
								  ,[LastName]
								  ,[Notes]
								  ,[PersonStatusId]
								  ,[EmployeeNumber]
								  ,[SeniorityId]
								  ,[ManagerId]
								  ,[PracticeOwnedId]
								  ,[IsDefaultManager]
								  ,[TelephoneNumber]
								  ,[IsWelcomeEmailSent]
								  ,[IsStrawman]
								  ,[IsOffshore]
								  ,[PaychexID]
								  ,[DivisionId]
								  ,[TerminationReasonId]
								  ,[RecruiterId]
								  ,[TitleId]
								  ,JobSeekerStatusId
								  ,SourceId
								  ,TargetedCompanyId
								  ,EmployeeReferralId
								  ,CohortAssignmentId
								  ,[CreatedDate]
								  ,[CreatedBy])
		SELECT i.[PersonId]
			  ,i.[HireDate]
			  ,i.[TerminationDate]
			  ,i.[Alias]
			  ,i.[DefaultPractice]
			  ,i.[FirstName]
			  ,i.[LastName]
			  ,i.[Notes]
			  ,i.[PersonStatusId]
			  ,i.[EmployeeNumber]
			  ,i.[SeniorityId]
			  ,i.[ManagerId]
			  ,i.[PracticeOwnedId]
			  ,i.[IsDefaultManager]
			  ,i.[TelephoneNumber]
			  ,i.[IsWelcomeEmailSent]
			  ,i.[IsStrawman]
			  ,i.[IsOffshore]
			  ,i.[PaychexID]
			  ,i.[DivisionId]
			  ,i.[TerminationReasonId]
			  ,i.[RecruiterId]
			  ,i.TitleId
			  ,i.JobSeekerStatusId
			  ,i.SourceId
			  ,i.TargetedCompanyId
			  ,i.EmployeeReferralId
			  ,i.CohortAssignmentId
			  ,@insertTime
			  ,l.PersonID
		FROM inserted i
		INNER JOIN dbo.SessionLogData AS l ON l.SessionID = @@SPID
		LEFT JOIN deleted d ON d.PersonId = i.PersonId
		WHERE ISNULL(i.HireDate,'') <> ISNULL(d.HireDate,'')
					OR ISNULL(i.TerminationDate,'') <> ISNULL(d.TerminationDate,'')
					OR ISNULL(i.Alias,'') <> ISNULL(d.Alias,'')
					OR ISNULL(i.DefaultPractice,'') <> ISNULL(d.DefaultPractice,'')
					OR ISNULL(i.FirstName,'') <> ISNULL(d.FirstName,'')
					OR ISNULL(i.LastName,'') <> ISNULL(d.LastName,'')
					OR ISNULL(i.Notes, '') <> ISNULL(d.Notes, '')
					OR ISNULL(i.PersonStatusId,'') <> ISNULL(d.PersonStatusId,'')
					OR ISNULL(i.EmployeeNumber,'') <> ISNULL(d.EmployeeNumber,'')
					OR ISNULL(i.SeniorityId,0) <> ISNULL(d.SeniorityId,0)
					OR ISNULL(i.ManagerId,0) <> ISNULL(d.ManagerId,0)
					OR ISNULL(i.PracticeOwnedId, 0) <> ISNULL(d.PracticeOwnedId, 0)
					OR ISNULL(i.IsDefaultManager,'') <> ISNULL(d.IsDefaultManager,'')
					OR ISNULL(i.TelephoneNumber,'') <> ISNULL(d.TelephoneNumber,'')
					OR ISNULL(i.IsWelcomeEmailSent, '') <> ISNULL(d.IsWelcomeEmailSent, '')
					OR ISNULL(i.IsStrawman, '') <> ISNULL(d.IsStrawman, '')
					OR ISNULL(i.IsOffshore, '') <> ISNULL(d.IsOffshore, '')
					OR ISNULL(i.PayChexID,'') <> ISNULL(d.PayChexID,'')
					OR ISNULL(i.DivisionId,0) <> ISNULL(d.DivisionId,0)
					OR ISNULL(i.IsOffshore,'') <> ISNULL(d.IsOffshore,'')
					OR ISNULL(i.TerminationReasonId, -1) <> ISNULL(d.TerminationReasonId, -1)
					OR ISNULL(i.[RecruiterId], -1) <> ISNULL(d.[RecruiterId], -1)
					OR ISNULL(i.[TitleId], -1) <> ISNULL(d.[TitleId], -1)	
					OR ISNULL(i.JobSeekerStatusId, -1) <> ISNULL(d.JobSeekerStatusId, -1)
					OR ISNULL(i.SourceId, -1) <> ISNULL(d.SourceId, -1)
					OR ISNULL(i.TargetedCompanyId, -1) <> ISNULL(d.TargetedCompanyId, -1)
					OR ISNULL(i.EmployeeReferralId, -1) <> ISNULL(d.EmployeeReferralId, -1)
					OR ISNULL(i.CohortAssignmentId, -1) <> ISNULL(d.CohortAssignmentId, -1)		
	END

		-- End logging session
	EXEC dbo.SessionLogUnprepare
END



