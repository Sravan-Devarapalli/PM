CREATE PROCEDURE [dbo].[GetBudgetActivityLog]
(
	@RequestId		INT
)
AS
BEGIN
SET NOCOUNT ON
	DECLARE @RequestIdLocal INT = @RequestId,
			@ProjectId	INT,
			@BeginDate	DATETIME = NULL,
			@EndDate	DATETIME,
			@LastApprovalRequestId INT =NULL

	SELECT @ProjectId=ProjectId
	FROM dbo.BudgetResetRequestHistory
	WHERE RequestId=@RequestIdLocal

	SELECT @EndDate=ApprovedDate
	FROM dbo.BudgetResetApprovalHistory
	WHERE RequestId=@RequestIdLocal

	;WITH CTELostApproval AS
	(
		SELECT RequestId, 
			   ProjectId, 
			   ROW_Number() OVER(ORDER BY RequestID DESC) AS RNo
			FROM BudgetResetRequestHistory
			WHERE ProjectId=@ProjectId AND RequestId<@RequestIdLocal
	)

	select @LastApprovalRequestId=a.RequestId, @BeginDate=b.ApprovedDate
	FROM CTELostApproval a
	join BudgetResetApprovalHistory b on a.RequestId=b.RequestId
	WHERE a.RNo=1


	SELECT MPH.ID as ActivityID,
		   MPH.LogTime as LogDate,
		   CASE WHEN (MPH.OLDStartDATE IS NULL AND MPH.OLDEndDate IS NULL AND MPH.OldHoursPerday IS NULL ANd MPH.OLDAmount Is NULL)THEN 'Added Resource to'
		        WHEN (MPH.NewStartDate IS NULL AND MPH.NewEndDate IS NULL AND MPH.NewHoursPerDay IS NULL ANd MPH.NewAmount Is NULL)THEN 'Deleted Resource to'
			ELSE 'Modified Resource to' END+' '+M.Description +'</br>'+
		   per.FirstName+' '+per.LastName AS ActivityName,

		   CASE WHEN (MPH.OldStartDate IS NULL ) THEN 'Start Date: '+ convert(varchar(10), cast(MPH.NewStartDate as date), 101)+'</br>'
				WHEN (MPH.OldStartDate IS NOT NULL AND MPH.OldStartDate != MPH.NewStartDate) THEN 'Start Date: '+convert(varchar(10), cast(MPH.OldStartDate as date), 101) +'=>'+ convert(varchar(10), cast(MPH.NewStartDate as date), 101) +'</br>'
				ELSE '' END +
				CASE WHEN (MPH.OldEndDate IS NULL ) THEN 'End Date: '+ convert(varchar(10), cast(MPH.NewEndDate as date), 101) +'</br>'
				WHEN (MPH.OldEndDate IS NOT NULL AND MPH.OldEndDate != MPH.NewEndDate) THEN 'End Date: '+convert(varchar(10), cast(MPH.OldEndDate as date), 101) +'=>'+convert(varchar(10), cast(MPH.NewEndDate as date), 101) +'</br>'
				ELSE '' END +
				CASE WHEN (MPH.OldAmount IS NULL ) THEN 'Amount: '+convert(varchar(25),MPH.NewAmount)+'</br>'
				WHEN (MPH.OldAmount IS NOT NULL AND MPH.OldAmount != MPH.NewAmount) THEN 'Amount: '+ convert(varchar(25),MPH.OldAmount)+'=>'+convert(varchar(25),MPH.NewAmount)+'</br>'
				ELSE '' END +
				CASE WHEN (MPH.OldHoursPerDay IS NULL ) THEN 'Hours Per Day: '+convert(varchar(25),MPH.NewHoursPerDay)+'</br>'
				WHEN (MPH.OldHoursPerDay IS NOT NULL AND MPH.OldHoursPerDay != MPH.NewHoursPerDay) THEN 'Hours Per Day: '+ convert(varchar(25),MPH.OldHoursPerDay)+'=>'+convert(varchar(25),MPH.NewHoursPerDay)+'</br>'
				ELSE '' END  AS LogData,
			P.PersonId,
			p.FirstName,
			p.LastName
	FROM MilestonePersonHistory MPH
	LEFT JOIN dbo.Person P on MPH.UpdatedBy=p.PersonId
	LEFT JOIN dbo.MilestonePerson MP On MP.MilestonePersonId=MPH.MilestonePersonId
	LEFT JOIN dbo.Person per ON per.PersonId=MPH.PersonId
	LEFT JOIN dbo.Milestone m on m.MilestoneId=mph.MilestoneId
	WHERE MPH.ProjectId=@ProjectId
	AND (@BeginDate IS NULL OR MPH.LogTime>=@BeginDate) AND (MPH.LogTime<=@EndDate)


END
