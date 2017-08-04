CREATE PROCEDURE [dbo].[GetBudgetResetToAddress]
(
	@ProjectId INT
)
AS
	SET NOCOUNT ON

	SELECT EIC.Alias+','+Manger.Alias+','+EM.Alias
	FROM Project p
	left join Person as EIC on p.ExecutiveInChargeId=EIC.PersonId
	left join Person as Manger on p.ProjectManagerId=Manger.PersonId
	LEFT JOIN Person as EM on p.EngagementManagerId=EM.PersonId
	WHERE p.ProjectId=@ProjectId

