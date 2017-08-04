CREATE PROCEDURE [dbo].[CleanBudgetRecordsForNonActiveProject]
(
	@ProjectId INT
)
AS
	
		UPDATE PBH
		SET PBH.IsActive=CAST(0 as BIT)
		FROM ProjectBudgetHistory PBH
		WHERE PBH.ProjectId = @ProjectId

		UPDATE Project
			SET Budget=NULL,
				BudgetSetDate=null
		WHERE ProjectId = @ProjectId

		DELETE PBPE
		FROM ProjectBudgetPersonEntry PBPE
		WHERE PBPE.ProjectId = @ProjectId

		DELETE PBME
		FROM ProjectBudgetMonthlyExpense PBME
		JOIN ProjectBudgetExpense PBE ON PBE.Id= PBME.ExpenseId
		WHERE PBE.ProjectId = @ProjectId
		
		DELETE PBE
		FROM ProjectBudgetExpense PBE
		WHERE PBE.ProjectId = @ProjectId

		DELETE PBFM
 		FROM ProjectBudgetFFMilestoneMonthlyRevenue PBFM
		JOIN ProjectBudgetHistory PBH ON PBH.MilestoneId = PBFM.MilestoneId 
		WHERE PBH.ProjectId = @ProjectId



