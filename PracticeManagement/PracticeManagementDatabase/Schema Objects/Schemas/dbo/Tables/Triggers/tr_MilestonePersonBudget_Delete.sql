CREATE TRIGGER [tr_MilestonePersonBudget_Delete]
ON [dbo].[MilestonePersonEntry]
AFTER DELETE
AS
	SET NOCOUNT ON

	DECLARE @UserLogin NVARCHAR(50)
	DECLARE @CurrentPMTime DATETIME 
	SET @CurrentPMTime = dbo.GettingPMTime(GETUTCDATE())
	
	SELECT @UserLogin = UserLogin
	FROM SessionLogData
	WHERE SessionID = @@SPID

	

	;with OLD_VALUES AS
	(
		SELECT m.ProjectId AS MilestoneProjectId,
		       mp.MilestoneId,
		       mp.MilestonePersonId,
			   d.StartDate,
   			   d.EndDate,
		       d.HoursPerDay,
			   d.Amount,
		       mp.PersonId,
			   d.id as MilestonePersonEntryId
		  FROM  deleted AS d
				INNER JOIN Milestoneperson mp on  d.MilestonePersonId = mp.MilestonePersonId 
		       INNER JOIN Milestone m on m.milestoneid=mp.milestoneid
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
				 MilestonePersonEntryId
	)
	SELECT  d.MilestoneProjectId,
		   d.MilestoneId ,
		   d.MilestonePersonId ,
		   d.StartDate,
		   null,
		   d.EndDate,
		   null,
		   d.HoursPerDay,
		   null,
		   d.Amount,
		   null,
		   @CurrentPMTime,
		   l.PersonID,
		   d.PersonId,
		   d.MilestonePersonEntryId
	FROM OLD_VALUES AS d 
	INNER JOIN dbo.SessionLogData AS l ON l.SessionID = @@SPID

	    

		IF  ( SELECT UserLogin FROM SessionLogData WHERE SessionID = @@SPID ) IS NULL
		BEGIN
			EXEC SessionLogPrepare @UserLogin = @UserLogin
		END
