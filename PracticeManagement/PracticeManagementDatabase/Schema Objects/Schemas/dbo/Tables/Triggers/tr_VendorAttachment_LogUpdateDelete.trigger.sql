﻿CREATE TRIGGER [tr_VendorAttachment_LogUpdateDelete]
ON [dbo].[VendorAttachment]
AFTER UPDATE, DELETE
AS
BEGIN
	-- Ensure the temporary table exists
	EXEC SessionLogPrepare @UserLogin = NULL
	
	DECLARE @CurrentPMTime DATETIME 
	SET @CurrentPMTime = dbo.InsertingTime()

	;WITH NEW_VALUES AS
	(
		SELECT i.Id
				,i.VendorId
				,v.Name as 'VendorName'
				,i.[FileName]
				,i.UploadedDate
		FROM inserted AS i
		LEFT JOIN Vendor v ON v.Id = i.VendorId
	),

	OLD_VALUES AS
	(
		SELECT d.Id
				,d.VendorId
				,v.Name as 'VendorName'
				,d.[FileName]
				,d.UploadedDate
		FROM deleted AS d 
		LEFT JOIN Vendor v ON v.Id = d.VendorId
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
	           WHEN d.Id IS NULL THEN 3 -- Actually is redundant
	           WHEN i.Id IS NULL THEN 5
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
					         FULL JOIN OLD_VALUES ON NEW_VALUES.Id = OLD_VALUES.Id
			           WHERE NEW_VALUES.Id = ISNULL(i.Id, d.Id) OR OLD_VALUES.Id = ISNULL(i.Id, d.Id)
					  FOR XML AUTO, ROOT('VendorAttachment'))),
			LogData = (SELECT *
					    FROM NEW_VALUES
					         FULL JOIN OLD_VALUES ON NEW_VALUES.Id = OLD_VALUES.Id
			           WHERE NEW_VALUES.Id = ISNULL(i.Id, d.Id) OR OLD_VALUES.Id = ISNULL(i.Id, d.Id)
					  FOR XML AUTO, ROOT('VendorAttachment'), TYPE),
			@CurrentPMTime
	  FROM inserted AS i
	       FULL JOIN deleted AS d ON i.Id = d.Id
	       INNER JOIN dbo.SessionLogData AS l ON l.SessionID = @@SPID
	 WHERE i.Id IS NULL -- deleted record
	    -- Detect changes
	    OR i.[FileName] <> d.[FileName]
	    OR i.UploadedDate <> d.UploadedDate
	-- End logging session
	 EXEC dbo.SessionLogUnprepare
END
