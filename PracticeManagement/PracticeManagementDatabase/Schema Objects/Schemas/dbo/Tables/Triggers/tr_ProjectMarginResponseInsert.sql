CREATE TRIGGER [tr_ProjectMarginResponseInsert]
ON [dbo].[MarginExceptionRequest]
AFTER UPDATE
AS
BEGIN
	EXEC SessionLogPrepare @UserLogin = NULL
	
	DECLARE @CurrentPMTime DATETIME 
	SET @CurrentPMTime = dbo.InsertingTime()

	;WITH NEW_VALUES AS
	(
		SELECT i.ProjectId
			  ,P.Name AS 'Name'
			  ,CASE WHEN i.TierOneStatus = 2 AND i.TierTwoStatus<>2 and i.TierTwoStatus<>3 THEN 'Accepted Tier 1 Margin Exception'
					WHEN i.TierOneStatus = 2 AND i.TierTwoStatus=2 THEN 'Accepted Tier 2 Margin Exception'
					WHEN i.TierOneStatus = 3 THEN 'Declined Tier 1 Margin Exception'
					WHEN i.TierOneStatus = 2 AND i.TierTwoStatus =3 THEN 'Declined Tier 2 Margin Exception' END
					as 'Activity',
			 i.Comments
		FROM inserted AS i
		INNER JOIN Project as P on p.ProjectId=i.ProjectId
	)

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
	SELECT 40 AS ActivityTypeID /* Margin only */,
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
					  FOR XML AUTO, ROOT('Margin'))),
			LogData = (SELECT NEW_VALUES.ProjectId
					    FROM inserted AS NEW_VALUES
			           WHERE NEW_VALUES.ProjectId = i.ProjectId
					  FOR XML AUTO, ROOT('Margin'), TYPE),
			@CurrentPMTime
	  FROM inserted AS i
	       INNER JOIN dbo.SessionLogData AS l ON l.SessionID = @@SPID
	  

	  ;WITH NEW_VALUES AS
	(
		SELECT i.ProjectId
			  ,P.Name AS 'Name'
			  ,CASE WHEN i.TierOneStatus = 2 AND i.TierTwoStatus<>2 and i.TierTwoStatus<>3  THEN 'Accepted Tier 1 Margin Exception'
					WHEN i.TierOneStatus = 2 AND i.TierTwoStatus=2 THEN 'Accepted Tier 2 Margin Exception'
					WHEN i.TierOneStatus = 3 THEN 'Declined Tier 1 Margin Exception'
					WHEN i.TierOneStatus = 2 AND i.TierTwoStatus =3 THEN 'Declined Tier 2 Margin Exception' END
					as 'Activity',
				i.Comments
		FROM inserted AS i
		INNER JOIN Project as P on p.ProjectId=i.ProjectId
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
	           40  ,l.SessionID,l.SystemUser,l.Workstation,l.ApplicationName,l.UserLogin,l.PersonID,l.LastName,l.FirstName,
		   Data = CONVERT(NVARCHAR(MAX),(SELECT NEW_VALUES.ProjectId,NEW_VALUES.Name,NEW_VALUES.Activity, NEW_VALUES.Comments
		               FROM NEW_VALUES
					        
					  FOR XML AUTO, ROOT('Margin'))),
			@CurrentPMTime
	  FROM  dbo.SessionLogData AS l 
	  WHERE  l.SessionID = @@SPID
	 
	  -- End logging session
	 EXEC dbo.SessionLogUnprepare
END

