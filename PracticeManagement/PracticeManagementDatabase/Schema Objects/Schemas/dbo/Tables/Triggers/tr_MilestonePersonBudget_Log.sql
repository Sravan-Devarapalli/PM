CREATE TRIGGER [tr_MilestonePersonBudget_Log]
ON [dbo].[MilestonePersonEntry]
AFTER INSERT, UPDATE
AS
	SET NOCOUNT ON

	DECLARE @UserLogin NVARCHAR(50)
	DECLARE @CurrentPMTime DATETIME 
	SET @CurrentPMTime = dbo.GettingPMTime(GETUTCDATE())
	
	SELECT @UserLogin = UserLogin
	FROM SessionLogData
	WHERE SessionID = @@SPID

	;WITH NEW_VALUES AS
	(
		SELECT mp.ProjectId AS MilestoneProjectId,
		       mp.MilestoneId,
		       mp.MilestonePersonId,
			   i.StartDate,
   			   i.EndDate,
		       i.HoursPerDay,
			   i.Amount,
		       mp.PersonId,
			   i.id as MilestonePersonEntryId,
			   i.PersonRoleId as RoleId
		  FROM inserted AS i
		       INNER JOIN dbo.v_MilestonePerson AS mp
		           ON i.MilestonePersonId = mp.MilestonePersonId 
	),

	OLD_VALUES AS
	(
		SELECT mp.ProjectId AS MilestoneProjectId,
		       mp.MilestoneId,
		       mp.MilestonePersonId,
			   d.StartDate,
   			   d.EndDate,
		       d.HoursPerDay,
			   d.Amount,
		       mp.PersonId,
			   d.id as MilestonePersonEntryId,
			   d.PersonRoleId as RoleId
		  FROM  deleted AS d
		       INNER JOIN dbo.v_MilestonePerson AS mp
		           ON d.MilestonePersonId = mp.MilestonePersonId 
	)

	INSERT INTO MilestonePersonHistory(
				 ProjectId,
				 MilestoneId,
				 MilestonePersonId,
				 OldStartDate,
				 NewStartDate,
				 OldEndDate,
				 NewEndDate,
				 OldHoursPerDay,
				 NewHoursPerDay,
				 OldAmount,
				 NewAmount,
				 LogTime,
				 UpdatedBy,
				 PersonId,
				 MilestonePersonEntryId,
				 RoleId
	)
	SELECT CASE WHEN (i.MilestoneProjectId IS NOT NULL) THEN i.MilestoneProjectId  ELSE d.MilestoneProjectId END AS MilestoneProjectId,
		   CASE WHEN (i.MilestoneId IS NOT NULL) THEN i.MilestoneId  ELSE d.MilestoneId END AS MilestoneId,
		   CASE WHEN (i.MilestonePersonId IS NOT NULL) THEN i.MilestonePersonId  ELSE d.MilestonePersonId END AS MilestonePersonId,
		   d.StartDate,
		   i.StartDate,
		   d.EndDate,
		   i.EndDate,
		   d.HoursPerDay,
		   i.HoursPerDay,
		   d.Amount,
		   i.Amount,
		   @CurrentPMTime,
		   l.PersonID,
		   CASE WHEN (i.PersonId IS NOT NULL) THEN i.PersonId  ELSE d.PersonId END AS PersonId,
		   CASE WHEN (i.MilestonePersonEntryId IS NOT NULL) THEN i.MilestonePersonEntryId  ELSE d.MilestonePersonEntryId END AS MilestonePersonEntryId,
		  i.RoleId
	FROM OLD_VALUES AS d 
	FULL JOIN NEW_VALUES i ON i.MilestonePersonId = d.MilestonePersonId
		 INNER JOIN dbo.SessionLogData AS l ON l.SessionID = @@SPID
	 WHERE i.MilestonePersonId IS NULL -- Deleted record
	    OR d.MilestonePersonId IS NULL -- Added record
		OR i.MilestoneId IS NULL
		OR d.MilestoneId IS NULL
		OR i.MilestoneProjectId IS NULL
	    OR ISNULL(i.Amount, 0) <> ISNULL(d.Amount, 0)
	    OR i.StartDate <> d.StartDate
		OR i.EndDate <> d.EndDate
	    OR i.HoursPerDay <> d.HoursPerDay
	    

		IF  ( SELECT UserLogin FROM SessionLogData WHERE SessionID = @@SPID ) IS NULL
		BEGIN
			EXEC SessionLogPrepare @UserLogin = @UserLogin
		END
