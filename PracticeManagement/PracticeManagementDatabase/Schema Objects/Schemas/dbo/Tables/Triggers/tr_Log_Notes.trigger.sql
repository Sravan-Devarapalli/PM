﻿-- =============================================
-- Author:		Nikita Goncharenko
-- Create date: 2010-07-08
-- Description:	Adds notes to AL
-- =============================================
CREATE TRIGGER [dbo].[tr_Log_Notes] 
   ON  [dbo].[Note] 
   AFTER INSERT, UPDATE
AS 
BEGIN
	SET NOCOUNT ON;

	 --Ensure the temporary table exists
	EXEC SessionLogPrepare @UserLogin = NULL

	--DECLARE @GMT NVARCHAR(10) = (SELECT Value FROM Settings WHERE SettingsKey = 'TimeZone')
	DECLARE @CurrentPMTime DATETIME
	SET @CurrentPMTime = dbo.InsertingTime()
	--(CASE WHEN CHARINDEX('-',@GMT) >0 THEN GETUTCDATE() - REPLACE(@GMT,'-','') ELSE 
	--										GETUTCDATE() + @GMT END)

	;WITH NEW_VALUES AS
	(
		SELECT  i.[NoteId]
			   ,i.[PersonId]
			   ,n.LastName + ', ' + n.FirstName as 'By'
			   ,i.[CreateDate]
			   ,i.[NoteText]
			   ,i.TargetId
			   ,n.NoteTargetId
			   ,n.NoteTargetName
			   ,m.ProjectId as ParentTargetId
			   ,(CASE WHEN n.NoteTargetId = 1 THEN m.Description 
						WHEN n.NoteTargetId = 2 THEN proj.Name 
						WHEN n.NoteTargetId = 3 THEN p.LastName + ', '+p.FirstName
						WHEN n.NoteTargetId = 4 THEN o.Name
						END ) AS NoteAddedTo
		FROM inserted AS i
			inner join v_Notes as n on n.NoteId = i.NoteId	
			left join dbo.Milestone m on n.NoteTargetId = 1 AND m.MilestoneId = i.TargetId
			  LEFT JOIN Project proj on n.NoteTargetId = 2 AND proj.ProjectId = i.TargetId
			  LEFT JOIN Person p on n.NoteTargetId = 3 AND p.PersonId = i.TargetId
			  LEFT JOIN Opportunity o on n.NoteTargetId = 4 AND o.OpportunityId = i.TargetId
	),

	OLD_VALUES AS
	(
		SELECT  d.[NoteId]
			   ,d.[PersonId]
			   ,n.LastName + ', ' + n.FirstName as 'By'
			   ,d.[CreateDate]
			   ,d.[NoteText]
			   ,d.TargetId
			   ,n.NoteTargetId
			   ,n.NoteTargetName
			   ,m.ProjectId as ParentTargetId
			   ,(CASE WHEN n.NoteTargetId = 1 THEN m.Description 
						WHEN n.NoteTargetId = 2 THEN proj.Name 
						WHEN n.NoteTargetId = 3 THEN p.LastName + ', '+p.FirstName
						WHEN n.NoteTargetId = 4 THEN o.Name
						END ) AS NoteAddedTo
		  FROM deleted AS d
			inner join v_Notes as n on n.NoteId = d.NoteId	
			left join dbo.Milestone m on n.NoteTargetId = 1 AND m.MilestoneId = d.TargetId
			  LEFT JOIN Project proj on n.NoteTargetId = 2 AND proj.ProjectId = d.TargetId
			  LEFT JOIN Person p on n.NoteTargetId = 3 AND p.PersonId = d.TargetId
			  LEFT JOIN Opportunity o on n.NoteTargetId = 4 AND o.OpportunityId = d.TargetId
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
	           WHEN d.[NoteId] IS NULL THEN 3
	           WHEN i.[NoteId] IS NULL THEN 5
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
	       Data = CONVERT(NVARCHAR(MAX),(SELECT *
					    FROM NEW_VALUES
					         FULL JOIN OLD_VALUES ON NEW_VALUES.[NoteId] = OLD_VALUES.[NoteId]
			           WHERE NEW_VALUES.[NoteId] = ISNULL(i.[NoteId], d.[NoteId]) OR OLD_VALUES.[NoteId] = ISNULL(i.[NoteId], d.[NoteId])
					  FOR XML AUTO, ROOT('Note'))),
		  LogData = (
						SELECT 
							NEW_VALUES.[NoteId],
							NEW_VALUES.[PersonId],
							NEW_VALUES.[By],
							NEW_VALUES.[CreateDate],
							NEW_VALUES.[TargetId],
							NEW_VALUES.[NoteTargetId],
							NEW_VALUES.[NoteTargetName],
							NEW_VALUES.[ParentTargetId],
							OLD_VALUES.[NoteId],
							OLD_VALUES.[PersonId],
							OLD_VALUES.[By],
							OLD_VALUES.[CreateDate],
							OLD_VALUES.[TargetId],
							OLD_VALUES.[NoteTargetId],
							OLD_VALUES.[NoteTargetName],
							OLD_VALUES.[ParentTargetId]
						FROM NEW_VALUES
								FULL JOIN OLD_VALUES ON NEW_VALUES.[NoteId] = OLD_VALUES.[NoteId]
						WHERE NEW_VALUES.[NoteId] = ISNULL(i.[NoteId], d.[NoteId]) OR OLD_VALUES.[NoteId] = ISNULL(i.[NoteId], d.[NoteId])
						FOR XML AUTO, ROOT('Note'), TYPE
					),
			@CurrentPMTime
	  FROM inserted AS i
	       FULL JOIN deleted AS d ON i.[NoteId] = d.[NoteId]
	       INNER JOIN dbo.SessionLogData AS l ON l.SessionID = @@SPID

	-- End logging session
	EXEC dbo.SessionLogUnprepare
END



