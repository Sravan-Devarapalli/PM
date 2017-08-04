CREATE TRIGGER [tr_ClientMarginGoal_Log]
ON [dbo].[ClientMarginGoal]
AFTER INSERT, UPDATE, DELETE
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
		SELECT i.ClientId,
			   i.Id,
			   CONVERT(NVARCHAR(10), i.StartDate, 101)  StartDate,
			   CONVERT(NVARCHAR(10), i.EndDate, 101)    EndDate,
			   i.MarginGoal,
			   i.Comments
		  FROM inserted AS i
	),

	OLD_VALUES AS
	(
		SELECT d.ClientId,
			   d.Id,
			   CONVERT(NVARCHAR(10), d.StartDate, 101)  StartDate,
			   CONVERT(NVARCHAR(10), d.EndDate, 101)    EndDate,
			   d.MarginGoal,
			   d.Comments
		FROM  deleted AS d
	)

	-- Activity 1 for added, 2 for changed, 3 for deleted

	INSERT INTO ClientMarginGoalHistory(
	ClientId,
	Activity,
	LogTime,
	PersonId,
	OldStartDate,
	NewStartDate,
	OldEndDate,
	NewEndDate,
	OldMarginGoal,
	NewMarginGoal,
	Comments
	)
	SELECT CASE WHEN d.ClientId IS NOT NULL THEN d.ClientId
		   ELSE i.ClientId END,
		   CASE WHEN d.Id IS NULL THEN 1
		        WHEN i.Id IS NULL THEN 3
				ELSE 2 END,
		   @CurrentPMTime,
		   l.PersonID,
		   d.StartDate,
		   i.StartDate,
		   d.EndDate,
		   i.EndDate,
		   d.MarginGoal,
		   i.MarginGoal,
		  CASE WHEN i.Comments IS NOT NULL THEN i.Comments
		  ELSE d.Comments END
	FROM OLD_VALUES AS d 
	FULL JOIN NEW_VALUES i ON i.Id = d.Id
		 INNER JOIN dbo.SessionLogData AS l ON l.SessionID = @@SPID
	WHERE i.Id IS NULL -- deleted record
		OR  d.Id IS NULL -- New record
		OR i.StartDate<>d.StartDate
		OR i.EndDate <> d.EndDate
		OR i.MarginGoal<>d.MarginGoal

	
		IF  ( SELECT UserLogin FROM SessionLogData WHERE SessionID = @@SPID ) IS NULL
		BEGIN
			EXEC SessionLogPrepare @UserLogin = @UserLogin
		END
