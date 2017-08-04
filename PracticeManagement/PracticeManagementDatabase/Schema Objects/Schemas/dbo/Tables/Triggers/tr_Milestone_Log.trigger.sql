﻿-- =============================================
-- Author:		Anatoliy Lokshin
-- Create date: 11-27-2008
-- Updated by:	
-- Update date:	
-- Description:	Logs the changes in the dbo.Milestone table.
-- =============================================
CREATE TRIGGER [dbo].[tr_Milestone_Log]
ON [dbo].[Milestone]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
	-- Ensure the temporary table exists
	EXEC SessionLogPrepare @UserLogin = NULL
	
	DECLARE @CurrentPMTime DATETIME ,@UserLogin NVARCHAR(50)
	SET @CurrentPMTime = dbo.InsertingTime()
	SELECT @UserLogin = UserLogin FROM SessionLogData WHERE SessionID = @@SPID

	;WITH NEW_VALUES AS
	(
		SELECT i.MilestoneId,
		       i.Description AS Name,
		       i.ProjectId AS MilestoneProjectId,
		       p.Name AS ProjectName,
			   CONVERT(NVARCHAR(10), i.ProjectedDeliveryDate, 101) AS 'ProjectedDeliveryDate',
   			   CONVERT(NVARCHAR(10), i.StartDate, 101) AS 'StartDate',
		       i.Amount,
			   CASE WHEN i.ConsultantsCanAdjust = 1 THEN 'Yes' ELSE 'No' END AS 'ConsultantsCanAdjust',
   			   CASE WHEN i.IsChargeable = 1 THEN 'Yes' ELSE 'No' END AS 'IsChargeable',
   			   CASE WHEN i.IsHourlyAmount = 1 THEN 'Yes' ELSE 'No' END AS 'IsHourlyAmount',
			   CASE WHEN i.MilestoneType =1 THEN 'General' ELSE 'Change Order' END AS 'MilestoneType',
			   CASE WHEN i.DiscountType = 1 THEN '$'+CONVERT(NVARCHAR(10),i.Discount)
					WHEN i.DiscountType = 2 THEN CONVERT(NVARCHAR(10),i.Discount)+'%' END AS 'Discount'
		  FROM inserted AS i
		       INNER JOIN dbo.Project AS p ON i.ProjectId = p.ProjectId
	),

	OLD_VALUES AS
	(
		SELECT d.MilestoneId,
		       d.Description AS Name,
		       d.ProjectId AS MilestoneProjectId,
		       p.Name AS ProjectName,
		       CONVERT(NVARCHAR(10), d.ProjectedDeliveryDate, 101) AS 'ProjectedDeliveryDate',
   			   CONVERT(NVARCHAR(10), d.StartDate, 101) AS 'StartDate',
		       d.Amount,
			   CASE WHEN d.ConsultantsCanAdjust = 1 THEN 'Yes' ELSE 'No' END AS 'ConsultantsCanAdjust',
   			   CASE WHEN d.IsChargeable = 1 THEN 'Yes' ELSE 'No' END AS 'IsChargeable',
   			   CASE WHEN d.IsHourlyAmount = 1 THEN 'Yes' ELSE 'No' END AS 'IsHourlyAmount',
			   CASE WHEN d.MilestoneType =1 THEN 'General' ELSE 'Change Order' END AS 'MilestoneType',
			   CASE WHEN d.DiscountType = 1 THEN '$'+CONVERT(NVARCHAR(10),d.Discount)
					WHEN d.DiscountType = 2 THEN CONVERT(NVARCHAR(10),d.Discount)+'%' END AS 'Discount'
		  FROM deleted AS d
		       INNER JOIN dbo.Project AS p ON d.ProjectId = p.ProjectId
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
	           WHEN d.MilestoneId IS NULL THEN 3
	           WHEN i.MilestoneId IS NULL THEN 5
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
					         FULL JOIN OLD_VALUES ON NEW_VALUES.MilestoneId = OLD_VALUES.MilestoneId
			           WHERE NEW_VALUES.MilestoneId = ISNULL(i.MilestoneId, d.MilestoneId) OR OLD_VALUES.MilestoneId = ISNULL(i.MilestoneId, d.MilestoneId)
					  FOR XML AUTO, ROOT('Milestone'))),
			LogData = (SELECT   NEW_VALUES.MilestoneId,
								NEW_VALUES.MilestoneProjectId,
								NEW_VALUES.ProjectedDeliveryDate,
								NEW_VALUES.StartDate,
								NEW_VALUES.MilestoneType,
								NEW_VALUES.Discount,
								OLD_VALUES.MilestoneId,
								OLD_VALUES.MilestoneProjectId,
								OLD_VALUES.ProjectedDeliveryDate,
								OLD_VALUES.StartDate,
								OLD_VALUES.MilestoneType,
								OLD_VALUES.Discount
					    FROM NEW_VALUES
					         FULL JOIN OLD_VALUES ON NEW_VALUES.MilestoneId = OLD_VALUES.MilestoneId
			           WHERE NEW_VALUES.MilestoneId = ISNULL(i.MilestoneId, d.MilestoneId) OR OLD_VALUES.MilestoneId = ISNULL(i.MilestoneId, d.MilestoneId)
					  FOR XML AUTO, ROOT('Milestone'), TYPE),
			@CurrentPMTime

	  FROM inserted AS i
	       FULL JOIN deleted AS d ON i.MilestoneId = d.MilestoneId
	       INNER JOIN dbo.SessionLogData AS l ON l.SessionID = @@SPID
	 WHERE i.MilestoneId IS NULL -- Deleted record
	    OR d.MilestoneId IS NULL -- Added record
	    OR ISNULL(i.Description, '') <> ISNULL(d.Description, '')
	    OR ISNULL(i.Amount, 0) <> ISNULL(d.Amount, 0)
	    OR i.StartDate <> d.StartDate
	    OR i.ProjectedDeliveryDate <> d.ProjectedDeliveryDate
	    OR i.IsHourlyAmount <> d.IsHourlyAmount
		OR i.MilestoneType <> d.MilestoneType
		OR (i.Discount IS NOT NULL AND d.Discount IS NOT NULL)
		OR i.Discount <> d.Discount
		IF  ( SELECT UserLogin FROM SessionLogData WHERE SessionID = @@SPID ) IS NULL
		BEGIN
			EXEC SessionLogPrepare @UserLogin = @UserLogin
		END
END


