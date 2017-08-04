CREATE TRIGGER [tr_ProjectBudgetResetInsert]
ON [dbo].[BudgetResetApprovalHistory]
AFTER INSERT
AS
BEGIN
	-- Ensure the temporary table exists
	EXEC SessionLogPrepare @UserLogin = NULL
	
	DECLARE @CurrentPMTime DATETIME 
	SET @CurrentPMTime = dbo.InsertingTime()

	;WITH NEW_VALUES AS
	(
		SELECT BRH.ProjectId
			  ,P.Name AS 'Name'
			  ,'Reset Budget' as 'Activity'
			  ,B.Name as 'ResetType'
			  ,CASE WHEN i.BudgetToDate IS NOT NULL THEN CONVERT(VARCHAR(10), i.BudgetToDate, 101) ELSE '-' END AS  BudgetToDate
		FROM inserted AS i
		INNER JOIN BudgetResetRequestHistory as BRH on i.RequestId=BRH.RequestId
		INNER JOIN Project as P on p.ProjectId=BRH.ProjectId
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
					  FOR XML AUTO, ROOT('Budget'))),
			LogData = (SELECT NEW_VALUES.ProjectId
							FROM NEW_VALUES
					  FOR XML AUTO, ROOT('Budget'), TYPE),
			@CurrentPMTime
	  FROM inserted AS i
	       INNER JOIN dbo.SessionLogData AS l ON l.SessionID = @@SPID
	  
	  --
	  	;WITH NEW_VALUES AS
	(
		SELECT BRH.ProjectId
			  ,P.Name AS 'Name'
			  ,'Reset Budget' as 'Activity'
			  ,B.Name as 'ResetType'
			  ,CASE WHEN i.BudgetToDate IS NOT NULL THEN CONVERT(VARCHAR(10), i.BudgetToDate, 101) ELSE '-' END AS  BudgetToDate
		FROM inserted AS i
		INNER JOIN BudgetResetRequestHistory as BRH on i.RequestId=BRH.RequestId
		INNER JOIN Project as P on p.ProjectId=BRH.ProjectId
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
		   Data = CONVERT(NVARCHAR(MAX),(SELECT NEW_VALUES.ProjectId, NEW_VALUES.Name,NEW_VALUES.Activity, NEW_VALUES.ResetType, NEW_VALUES.BudgetToDate
		               FROM NEW_VALUES
					        
					  FOR XML AUTO, ROOT('Budget'))),
			@CurrentPMTime
	  FROM  dbo.SessionLogData AS l 
	  WHERE  l.SessionID = @@SPID
	 
	  -- End logging session
	 EXEC dbo.SessionLogUnprepare
END


