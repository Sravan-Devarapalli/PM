CREATE TRIGGER [tr_ProjectBudgetRequestInsert]
ON [dbo].[BudgetResetRequestHistory]
AFTER INSERT
AS
BEGIN
	-- Ensure the temporary table exists
	EXEC SessionLogPrepare @UserLogin = NULL
	
	DECLARE @CurrentPMTime DATETIME 
	SET @CurrentPMTime = dbo.InsertingTime()

	;WITH NEW_VALUES AS
	(
		SELECT i.ProjectId
			  ,P.Name AS 'Name'
			  ,'Requested Project Budget reset' as 'Activity',
			  B.Name as 'ResetType',
			  CASE WHEN i.BudgetToDate IS NOT NULL THEN CONVERT(VARCHAR(10), i.BudgetToDate, 101) ELSE '-' END AS  BudgetToDate,
			  i.Comments
		FROM inserted AS i
		INNER JOIN Project as P on p.ProjectId=i.ProjectId
		JOIN BudgetResetType B on B.Id = i.ResetType
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
	SELECT 30 AS ActivityTypeID /* Budget only */,
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
										WHERE NEW_VALUES.ProjectId = i.ProjectId
					  FOR XML AUTO, ROOT('Budget'))),
			LogData = (SELECT NEW_VALUES.ProjectId
					    FROM inserted AS NEW_VALUES
			           WHERE NEW_VALUES.ProjectId = i.ProjectId
					  FOR XML AUTO, ROOT('Budget'), TYPE),
			@CurrentPMTime
	  FROM inserted AS i
	       INNER JOIN dbo.SessionLogData AS l ON l.SessionID = @@SPID
	  

	  ;WITH NEW_VALUES AS
	(
		SELECT i.ProjectId
			  ,P.Name AS 'Name'
			  ,'Requested Project Budget reset' as 'Activity',
			  B.Name as 'ResetType',
			   CASE WHEN i.BudgetToDate IS NOT NULL THEN CONVERT(VARCHAR(10), i.BudgetToDate, 101) ELSE '-' END AS  BudgetToDate,
			  i.Comments
		FROM inserted AS i
		INNER JOIN Project as P on p.ProjectId=i.ProjectId
		JOIN BudgetResetType B on B.Id = i.ResetType
	)

	  INSERT INTO dbo.UserActivityLogRecordPerChange
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
	             LogDate)
	SELECT 
	           30  ,l.SessionID,l.SystemUser,l.Workstation,l.ApplicationName,l.UserLogin,l.PersonID,l.LastName,l.FirstName,
		   Data = CONVERT(NVARCHAR(MAX),(SELECT NEW_VALUES.ProjectId,NEW_VALUES.Name,NEW_VALUES.Activity, NEW_VALUES.Comments, NEW_VALUES.ResetType, NEW_VALUES.BudgetToDate
		               FROM NEW_VALUES
					        
					  FOR XML AUTO, ROOT('Budget'))),
			@CurrentPMTime
	  FROM  dbo.SessionLogData AS l 
	  WHERE  l.SessionID = @@SPID
	 
	  -- End logging session
	 EXEC dbo.SessionLogUnprepare
END



