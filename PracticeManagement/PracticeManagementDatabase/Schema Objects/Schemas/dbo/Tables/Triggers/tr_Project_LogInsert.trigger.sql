﻿CREATE TRIGGER [dbo].[tr_Project_LogInsert]
ON [dbo].[Project]
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
				,i.ProjectNumber AS 'ProjectNumber'
				,i.Name AS 'Name'
				,i.ClientId
				,C.Name AS 'ClientName'
				,i.PracticeId
				,prac.Name AS 'PracticeArea'
				,i.ProjectStatusId
				,ps.Name AS 'ProjectStatusName'
				,i.Discount
				,i.BuyerName AS 'BuyerName'
				,i.GroupId
				,PG.Name AS 'ProjectGroup'
				,i.Description
				,i.ExecutiveInChargeId AS DirectorId
				,CASE WHEN i.ExecutiveInChargeId IS NOT NULL THEN D.LastName + ', ' + D.FirstName 
				      ELSE '' 
				      END AS 'ExecutiveInCharge'
				,CASE WHEN i.IsChargeable = 1 THEN 'Yes'
						ELSE 'No' END AS 'IsChargeable',
				i.ProjectManagerId AS ProjectOwnerId
				,ProjOwner.LastName + ', ' + ProjOwner.FirstName AS [ProjectManager]
				, i.SowBudget
				,i.BusinessTypeId
				,bt.Name AS [BusinessType]
				,i.PricingListId
				,pt.Name AS [PricingList]
				,i.EngagementManagerId AS SeniorManagerId
				,CASE WHEN i.IsSeniorManagerUnassigned = 1 THEN 'Unassigned' ELSE SenManager.LastName + ', ' + SenManager.FirstName END AS EngagementManager
				,i.[ReviewerId]
				,Rev.LastName + ', ' + Rev.FirstName AS [Reviewer]
				,i.PONumber
				,CASE WHEN i.ExceptionMargin IS NOT NULL THEN CONVERT(nvarchar(10),i.ExceptionMargin)+'%'
					  ELSE '' END AS ExceptionMargin
				,CASE WHEN i.Budget IS NOT NULL THEN '$'+PARSENAME(CONVERT(VARCHAR,CAST(i.Budget AS MONEY),1),2)
					  ELSE '' END AS 'BudgetAmount'
		FROM inserted AS i
		INNER JOIN dbo.Client AS C ON C.ClientId = i.ClientId
		INNER JOIN dbo.Practice AS prac ON prac.PracticeId = i.PracticeId
		INNER JOIN dbo.ProjectStatus AS ps ON ps.ProjectStatusId = i.ProjectStatusId
		INNER JOIN dbo.ProjectGroup AS PG ON PG.GroupId = i.GroupId
		LEFT JOIN dbo.Person AS ProjOwner ON ProjOwner.PersonId = i.ProjectManagerId -- While Converting opportunity to Project ProjectOwnerId will not be there.So here Left join is used instead of INNER JOIN.
		LEFT JOIN dbo.Person AS SenManager ON SenManager.PersonId = i.EngagementManagerId
		LEFT JOIN dbo.Person AS Rev ON Rev.PersonId = i.[ReviewerId]
		LEFT JOIN dbo.Person AS D ON D.PersonId = i.ExecutiveInChargeId
		LEFT JOIN dbo.BusinessType bt ON bt.BusinessTypeId = i.BusinessTypeId
		LEFT JOIN dbo.PricingList pt ON pt.PricingListId = i.PricingListId
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
	SELECT 3 AS ActivityTypeID /* insert only */,
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
					  FOR XML AUTO, ROOT('Project'))),
			LogData = (SELECT NEW_VALUES.ProjectId
					    FROM inserted AS NEW_VALUES
			           WHERE NEW_VALUES.ProjectId = i.ProjectId
					  FOR XML AUTO, ROOT('Project'), TYPE),
			@CurrentPMTime
	  FROM inserted AS i
	       INNER JOIN dbo.SessionLogData AS l ON l.SessionID = @@SPID
	  
	  INSERT INTO [dbo].[ProjectStatusHistory]
			   ([ProjectId]
			   ,[ProjectStatusId]
			   ,[StartDate]
			   )
     SELECT i.ProjectId,i.ProjectStatusId,CONVERT(DATE,@CurrentPMTime)
	 FROM inserted AS i

	  -- End logging session
	 EXEC dbo.SessionLogUnprepare
END



